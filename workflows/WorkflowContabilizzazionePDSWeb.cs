using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowContabilizzazionePDSWeb : Workflow
    {
        private Action<StateContext> _DrawPage { get; set; }

        private List<string> ShowMethods(Type type)
        {
            List<string> methods = new List<string>();

            foreach (var method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (method.Name.StartsWith("_AddActivity_")) methods.Add(method.Name);
            }

            return methods;
        }


        public WorkflowContabilizzazionePDSWeb(string key, string title, Action<StateContext> drawPage) : base(key, title)
        {
            _DrawPage = drawPage;

            List<string> methods = ShowMethods(typeof(WorkflowContabilizzazionePDSWeb));

            foreach (string s in methods)
            {
                MethodInfo m = this.GetType().GetMethod(s, BindingFlags.NonPublic | BindingFlags.Instance);
                m.Invoke(this, new object[] { this });
            }
        }

        private void _AddActivity_Soggetto(Workflow wf)
        {
            Activity a = wf.CreateActivity("sogg");
            a.Title = "Quale tipo di soggetto vuoi abilitare?";
            a.TestoRiepilogo = "Tipo di soggetto:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("prof", "Professionista"),
                new InputItem("azi", "Azienda"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("articoliStudio");
            b1.Condition.IfOutputContainsItem("prof");

            Branch b2 = a.CreateBranchTo("articoliAzienda");
            b2.Condition.IfOutputContainsItem("azi");
        }

        private void _AddActivity_Modulo(Workflow wf)
        {
            Activity a = wf.CreateActivity("articoliStudio");
            a.Title = "Quale configurazione vuoi attivare?";
            a.Title = "Configurazione da attivare";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("9330033.SAAS","9330033.SAAS - Contabilizzazione fatture fino a 3 PdL" ,"9330033.SAAS"),
                new InputItem("9330053.SAAS","9330053.SAAS - Contabilizzazione fatture fino a 5 PdL"  ,"9330053.SAAS"  ),
                new InputItem("9330103.SAAS","9330103.SAAS - Contabilizzazione fatture fino a 10 PdL" ,"9330103.SAAS"  ),
                new InputItem("9330993.SAAS","9330993.SAAS - Contabilizzazione fatture oltre 10 PdL"  ,"9330993.SAAS"  )
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_ModuloAzi(Workflow wf)
        {
            Activity a = wf.CreateActivity("articoliAzienda");
            a.Title = "Quale configurazione vuoi attivare?";
            a.Title = "Configurazione da attivare";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("9330013.SAAS", "9330013.SAAS - Contabilizzazione fatture per azienda", "9330013.SAAS"),
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
            a.DrawPage = _DrawPage;
        }
    }
}