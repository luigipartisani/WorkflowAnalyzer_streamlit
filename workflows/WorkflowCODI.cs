using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowCODI : Workflow
    {
        private Action<StateContext> _DrawPage { get; set; }

        private int tipoLicenza { get; set; } //0 -> niente, 1-> comm, 2 -> azi

        private List<string> ShowMethods(Type type)
        {
            List<string> methods = new List<string>();

            foreach (var method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (method.Name.StartsWith("_AddActivity_")) methods.Add(method.Name);
            }

            return methods;
        }

        public WorkflowCODI(string key, string title, Action<StateContext> drawPage, int tipoLicenza) : base(key, title)
        {
            _DrawPage = drawPage;

            List<string> methods = ShowMethods(typeof(WorkflowCODI));

            this.tipoLicenza = tipoLicenza;

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
                new InputItem("demo", "Demo - fino al " + DateTime.Now.AddDays(15).ToShortDateString()),
                new InputItem("standard","Standard")
       }));
            a.DrawPage = _DrawPage;

            Branch b1 = null;
            if (tipoLicenza == 0)
            {
                b1 = a.CreateBranchTo("tipoCliente");
            }
            else
            {
                b1 = a.CreateBranchTo("azienda");
            }
        }

        private void _AddActivity_TipoCliente(Workflow wf)
        {
            Activity a = wf.CreateActivity("tipoCliente");
            a.Title = "Quale tipo di soggetto vuoi abilitare?";
            a.TestoRiepilogo = "Tipo di soggetto:";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("prof", "Professionista", "PDS.COMM"),
                new InputItem("azi", "Azienda", "PDS.AZI"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("azienda");
        }

        private void _AddActivity_AttivaAzienda(Workflow wf)
        {
            Activity a = wf.CreateActivity("azienda");
            a.Title = "Quale versione desideri attivare?";
            a.TestoRiepilogo = "Versione da attivare:";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
               new InputItem("CODI.001", "CODI.001 - Controllo d'Impresa - Monoazienda", "CODI.001"),
               new InputItem("CODI.005", "CODI.005 - Controllo d'Impresa - fino a 5 aziende", "CODI.005"),
               new InputItem("CODI.010", "CODI.010 - Controllo d'Impresa - fino a 10 aziende","CODI.010"),
               new InputItem("CODI.025", "CODI.025 - Controllo d'Impresa - fino a 25 aziende","CODI.025"),
               new InputItem("CODI.050", "CODI.050 - Controllo d'Impresa - fino a 50 aziende","CODI.050"),
               new InputItem("CODI.099", "CODI.099 - Controllo d'Impresa - aziende ilimitate","CODI.099"),
            }));
            a.DrawPage = _DrawPage;

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