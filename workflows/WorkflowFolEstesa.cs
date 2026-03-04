using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowFolEstesa : Workflow
    {
        public int qtaAttivazione { get; set; }

        private Action<StateContext> _DrawPage { get; set; }

        private List<string> GetActivities(Type type)
        {
            List<string> activities = new List<string>();

            foreach (var method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (method.Name.StartsWith("_AddActivity_")) activities.Add(method.Name);
            }

            return activities;
        }

        public WorkflowFolEstesa(string key, string title, Action<StateContext> drawPage, int qta) : base(key, title)
        {
            _DrawPage = drawPage;

            List<string> activities = GetActivities(typeof(WorkflowFolEstesa));

            this.qtaAttivazione = qta;

            foreach (string a in activities)
            {
                MethodInfo m = this.GetType().GetMethod(a, BindingFlags.NonPublic | BindingFlags.Instance);
                m.Invoke(this, new object[] { this });
            }
        }

        private void _AddActivity_TipoLicenza(Workflow wf)
        {
            Activity a = wf.CreateActivity("lic");
            a.Title = "Che tipo di licenza desideri attivare?";
            a.TestoRiepilogo = "Tipo di licenza da attivare:";
            //a.Description = "Breve descrizione...";
            //a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
            //     new InputItem("{'Key':'tipoLicenza','Text':'Demo','DataType':'radioText', 'Tag':'tipoLicenza','Style':'hidden','Index':0}"),
            //      new InputItem("{'Key':'tipoLicenza','Text':'Standard','DataType':'radioText', 'Tag':'tipoLicenza', 'Style':'hidden','Index':1}"),
            //}));

            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[]
{
                //new InputItem("demo", "Demo - fino al " + DateTime.Now.AddDays(7).ToShortDateString()),
                 new InputItem("demo", "Demo - fino al " + DateTime.Now.AddDays(15).ToShortDateString()),
                new InputItem("standard","Standard")
}));

            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("tipoAttivazione");
        }

        private void _AddActivity_TipoAttivazione(Workflow wf)
        {
            Activity a = wf.CreateActivity("tipoAttivazione");
            a.Title = "Che tipo di attivazione desideri effettuare?";
            a.TestoRiepilogo = "Tipo di attivazione";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("nuova", "Nuova attivazione"),
                new InputItem("upgrade", "Upgrade"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("moduli");
            b1.Condition.IfOutputContainsItem("nuova");

            Branch b2 = a.CreateBranchTo("moduliUpgrade");
            b2.Condition.IfOutputContainsItem("upgrade");
        }

        private void _AddActivity_TipoModuli(Workflow wf)
        {
            Activity a = wf.CreateActivity("moduli");
            a.Title = "Quali moduli desideri attivare?<br/><span style='font-size:14px'>Per l'attivazione dei moduli è necessario che il cliente sia già in possesso di Fatture Online.</span>";
            a.TestoRiepilogo = "Moduli da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                new InputItem("9000300", "Scadenze", "9000300"),
                new InputItem("9000400", "RiBa", "9000400"),
                new InputItem("9000500", "Documenti ricorsivi", "9000500"),
                new InputItem("9000600", "Magazzino", "9000600"),
                new InputItem("9000700", "App", "9000700"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("attivazioni");
        }

		private void _AddActivity_TipoModuliUpgrade(Workflow wf)
		{
			Activity a = wf.CreateActivity("moduliUpgrade");
			a.Title = "Quali moduli desideri attivare?<br/><span style='font-size:14px'>Per l'attivazione dei moduli è necessario che il cliente sia già in possesso di Fatture Online. 1/2</span>";
			a.TestoRiepilogo = "Moduli da attivare:";
			//a.Description = "Breve descrizione...";
			a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
				new InputItem("9000300", "Scadenze", "9000300"),
				new InputItem("9000400", "RiBa", "9000400"),
				new InputItem("9000500", "Documenti ricorsivi", "9000500"),
				new InputItem("9000600", "Magazzino", "9000600"),
				new InputItem("9000700", "App", "9000700"),
			}));
            a.AllowNoChoice = true;
			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("attivazioniUpgrade");
		}

		private void _AddActivity_NumeroAttivazioni(Workflow wf)
        {
            Activity a = wf.CreateActivity("attivazioni");
            a.Title = "Quante attivazioni desideri assegnare al cliente?";
            a.TestoRiepilogo = "Numero di attivazioni:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
                new InputItem("{'Key':'numero','Text':'Numero di attivazioni','DataType':'integer','MinValue':1,'MaxValue':" + qtaAttivazione + ",'DefaultValue':1}"),
                //new InputItem("{'Key':'dec','Text':'Decimale','DataType':'decimal','MinValue':6.4,'MaxValue':13.46,'DefaultValue':7.76}"),
                //new InputItem("{'Key':'dat','Text':'Data','DataType':'date','MinValue':'2000-01-01','MaxValue':'2099-12-31','DefaultValue':'" + DateTime.Now.ToString("yyyy-MM-dd") + "'}"),
                //new InputItem("{'Key':'str','Text':'Testo','DataType':'text','DefaultValue':'Luigi'}"),
            }));
            a.DrawPage = _DrawPage;

            //Branch b1 = a.CreateBranchToSummary();
            Branch b1 = a.CreateBranchTo("uploadFile");
        }

		private void _AddActivity_NumeroAttivazioniUpgrade(Workflow wf)
		{
			Activity a = wf.CreateActivity("attivazioniUpgrade");
			a.Title = "Quante attivazioni desideri assegnare al cliente?";
			a.TestoRiepilogo = "Numero di attivazioni:";
			//a.Description = "Breve descrizione...";
			a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
				new InputItem("{'Key':'numero','Text':'Numero di attivazioni','DataType':'integer','MinValue':1,'MaxValue':" + qtaAttivazione + ",'DefaultValue':1}"),
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
            a.Title = "La procedura di attivazione si è conclusa";
            a.DrawPage = _DrawPage;
        }
    }
}
