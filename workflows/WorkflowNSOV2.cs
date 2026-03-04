using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowNSOV2 : Workflow
    {
        private Action<StateContext> _DrawPage { get; set; }
        private int tipoLicenza { get; set; } // 0 -> non presente su cat_inv_reg, 1 -> presente

        private List<string> ShowMethods(Type type)
        {
            List<string> methods = new List<string>();

            foreach (var method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (method.Name.StartsWith("_AddActivity_")) methods.Add(method.Name);
            }

            return methods;
        }


        public WorkflowNSOV2(string key, string title, Action<StateContext> drawPage, int tipoLicenza) : base(key, title)
        {
            _DrawPage = drawPage;

            this.tipoLicenza = tipoLicenza;

            List<string> methods = ShowMethods(typeof(WorkflowNSOV2));

            foreach (string s in methods)
            {
                MethodInfo m = this.GetType().GetMethod(s, BindingFlags.NonPublic | BindingFlags.Instance);
                m.Invoke(this, new object[] { this });
            }
        }

        private void _AddActivity_TipoLicenza(Workflow wf)
        {
            Activity a = wf.CreateActivity("lic");
            a.Title = "Che tipo di licenza desideri attivare?";
            a.TestoRiepilogo = "Tipo di licenza da attivare:";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[]
            {
                //new InputItem("demo", "Demo - fino al " + DateTime.Now.AddDays(7).ToShortDateString()),
                new InputItem("demo", "Demo - fino al " + DateTime.Now.AddDays(15).ToShortDateString()),
                new InputItem("standard","Standard")
            }));
            
            a.DrawPage = _DrawPage;

            Branch b1 = null;
            if (tipoLicenza == 0)
            {
                b1 = a.CreateBranchTo("tipoSogg");
            }
            else
            {
                b1 = a.CreateBranchTo("attivaNSO");
            }
        }

        private void _AddActivity_Soggetto(Workflow wf)
        {
            Activity a = wf.CreateActivity("tipoSogg");
            a.Title = "Quale tipo di soggetto vuoi abilitare?";
            a.TestoRiepilogo = "Tipo di soggetto:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("prof", "Professionista"),
                new InputItem("azi", "Azienda"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("attivaNSO");
        }

        private void _AddActivity_AttivaNSO(Workflow wf)
        {
            Activity a = wf.CreateActivity("attivaNSO");
            a.Title = "Quale modulo desideri attivare?";
            a.TestoRiepilogo = "Modulo da attivare:";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("9100100", "9100100 - Attivazione ordini NSO e PEPPOL","9100100", true)
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("attivaNSOPA");
        }

		private void _AddActivity_AttivaNSOPag2(Workflow wf)
		{
			Activity a = wf.CreateActivity("attivaNSOPA");
			a.Title = "Vuoi acquistare pacchetti di fatture/ordini PA?";
			a.TestoRiepilogo = "Pacchetti di fatture/ordini PA:";
			a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
				new InputItem("9112093", "9112093 - 12 fatture/ordini PA per anno","9112093"),
                new InputItem("9124093", "9124093 - 24 fatture/ordini PA per anno","9124093"),
                new InputItem("9136093", "9136093 - 36 fatture/ordini PA per anno","9136093"),
                new InputItem("9110093", "9110093 - 100 fatture/ordini PA per anno","9110093"),
                new InputItem("9115003", "9115003 - 500 fatture/ordini PA per anno","9115003")
			}));

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