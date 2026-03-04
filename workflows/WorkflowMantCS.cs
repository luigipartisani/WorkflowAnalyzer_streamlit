using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowMantCS : Workflow
    {
        //public bool possiedeContratto { get; set; }
        private Action<StateContext> _DrawPage { get; set; }
        private int tipoLicenza { get; set; } // 0 -> niente, 1 -> comm, 2 -> azi

        private List<string> ShowMethods(Type type)
        {
            List<string> methods = new List<string>();

            foreach (var method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (method.Name.StartsWith("_AddActivity_")) methods.Add(method.Name);
            }

            return methods;
        }


        public WorkflowMantCS(string key, string title, Action<StateContext> drawPage) : base(key, title)
        {
            _DrawPage = drawPage;

            List<string> methods = ShowMethods(typeof(WorkflowMantCS));

            foreach (string s in methods)
            {
                MethodInfo m = this.GetType().GetMethod(s, BindingFlags.NonPublic | BindingFlags.Instance);
                m.Invoke(this, new object[] { this });
            }
        }

        private void _AddActivity_MantenimentoCS(Workflow wf)
        {
            Activity a = wf.CreateActivity("mantenimentoCS");
            a.Title = "Mantenere la conservazione?";
            a.TestoRiepilogo = "Mantenere la conservazione:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
				new InputItem("910","910 - Mantenimento in Conservazione Sostitutiva sul Portale dei Servizi ","910", true),
				new InputItem("911","911 - Mantenimento in conservazione sostitutiva senza disdetta PdS","911"),
				new InputItem("917","917 - Mantenimento in Conservazione Sostitutiva su Condiviso","917"),
            }));
            a.DrawPage = _DrawPage;
            a.AllowNoChoice = true;

            Branch b1 = a.CreateBranchTo("infoMantenimentoCS910");
            b1.Condition.IfOutputContainsItem("910");

			Branch b2 = a.CreateBranchTo("infoMantenimentoCS911");
			b2.Condition.IfOutputContainsItem("911");

            Branch b3 = a.CreateBranchTo("infoMantenimentoCS917");
			b3.Condition.IfOutputContainsItem("917");
		}

		private void _AddActivity_InfoMantenimentoCS910(Workflow wf)
		{
			Activity a = wf.CreateActivity("infoMantenimentoCS910");
			a.Title = "<span style='font-size:14px'>Il modulo può essere attivato qualora il cliente in oggetto abbia disdetto<br/>la Conservazione B2b o Pa unitamente al Portale dei Servizi.<br/>Consente l'accesso esterno al sito del conservatore per la visualizzazione delle fatture conservate.</span>";
			a.TestoRiepilogo = "";

			a.DrawPage = _DrawPage;
			a.AllowNoChoice = true;

			Branch b1 = a.CreateBranchTo("uploadFile");
		}


		private void _AddActivity_InfoMantenimentoCS911(Workflow wf)
		{
			Activity a = wf.CreateActivity("infoMantenimentoCS911");
			a.Title = "<span style='font-size:14px'>Il modulo può essere attivato qualora il cliente in oggetto abbia disdetto<br/>la Conservazione B2b e Pa e NON il Portale dei Servizi.<br/>Consente l'accesso alla tile di conservazione sul Portale dei servizi.</span>";
			a.TestoRiepilogo = "";

			a.DrawPage = _DrawPage;
			a.AllowNoChoice = true;

			Branch b1 = a.CreateBranchTo("uploadFile");
		}


        private void _AddActivity_InfoMantenimentoCS917(Workflow wf)
        {
			Activity a = wf.CreateActivity("infoMantenimentoCS917");
			a.Title = "<span style='font-size:14px'>Il modulo può essere attivato qualora il cliente in oggetto abbia disdetto Condiviso Conservazione.<br/>Consente l'accesso alla tile di conservazione su Condiviso.</span>";
			a.TestoRiepilogo = "";

			a.DrawPage = _DrawPage;
			a.AllowNoChoice = true;

			Branch b1 = a.CreateBranchTo("uploadFile");
        }

		private void _AddActivity_UploadPDF(Workflow wf)
        {
            Activity a = wf.CreateActivity("uploadFile");
            a.Title = "Carica il pdf del contratto";
            a.TestoRiepilogo = "PDF del contratto:";
            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
                 new InputItem("{'Key':'uploadFile','Text':'Caricare un file PDF','DataType':'blob', 'Tag':'Blob'}"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchToSummary();
        }

        private void _AddActivity_Summary(Workflow wf)
        {
            Activity a = wf.CreateSummaryActivity();
            a.Title = "Vuoi procedere con l'attivazione?";
            a.DrawPage = _DrawPage;
        }

        private void _AddActivity_Outcome(Workflow wf)
        {
            Activity a = wf.CreateOutcomeActivity();
            a.DrawPage = _DrawPage;
        }

    }
}