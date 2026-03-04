using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowAccresco : Workflow
    {
        //private bool possiedeContratto { get; set; }

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

        public WorkflowAccresco(string key, string title, Action<StateContext> drawPage/*, bool possiedeContratto*/) : base(key, title)
        {
            _DrawPage = drawPage;

            List<string> activities = GetActivities(typeof(WorkflowAccresco));

            //this.possiedeContratto = possiedeContratto;

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
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                 new InputItem("Demo","Demo"),
                  new InputItem("Standard", "Standard"),
            }));
            a.DrawPage = _DrawPage;

            //Branch b1 = a.CreateBranchTo("tipoContratto");
            Branch b1 = a.CreateBranchTo("tipoContratto");
            b1.Condition.IfOutputContainsItem("Standard");

            Branch b2 = a.CreateBranchTo("bundleDemo");
            b2.Condition.IfOutputContainsItem("Demo");
        }

        private void _AddActivity_TipoContratto(Workflow wf)
        {
            Activity a = wf.CreateActivity("tipoContratto");
            a.Title = "Che tipo di contratto vuoi attivare?";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
               new InputItem("bundle", "Bundle"),
               new InputItem("upgrade", "Upgrade"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("bundle");
            b1.Condition.IfOutputContainsItem("bundle");

            Branch b2 = a.CreateBranchTo("upgradeAccresco");
            b2.Condition.IfOutputContainsItem("upgrade");
        }

        private void _AddActivity_NuovoContratto(Workflow wf)
        {
            Activity a = wf.CreateActivity("bundle");
            a.Title = "Quale modulo vuoi attivare?";
            a.TestoRiepilogo = "Modulo da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("4528005", "4528005 - Bundle Light (comprende Denuncia Iva periodica)","4528005"),
                new InputItem("4528025", "4528025 - Bundle Full (comprende Denuncia Iva periodica)","4528025"),
                new InputItem("4528105", "4528105 - Bundle Light (NON comprende Denuncia iva periodica)","4528105"),
                new InputItem("4528125", "4528125 - Bundle Full (NON comprende Denuncia iva periodica)","4528125")
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_NuovoContrattoDemo(Workflow wf)
        {
            Activity a = wf.CreateActivity("bundleDemo");
            a.Title = "Quale modulo vuoi attivare?";
            a.TestoRiepilogo = "Modulo da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("4528005", "4528005 - Bundle Light (comprende Denuncia Iva periodica)","4528005"),
                new InputItem("4528025", "4528025 - Bundle Full (comprende Denuncia Iva periodica)","4528025"),
                new InputItem("4528105", "4528105 - Bundle Light (NON comprende Denuncia iva periodica)","4528105"),
                new InputItem("4528125", "4528125 - Bundle Full (NON comprende Denuncia iva periodica)","4528125")
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_Upgrade(Workflow wf)
        {
            Activity a = wf.CreateActivity("upgradeAccresco");
            a.Title = "Quale modulo vuoi attivare? <span style='font-size:20px'>1 di 2</span>";
            a.TestoRiepilogo = "Modulo da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
                new InputItem("{'Key':'postazioniLavoroAccresco','Text':'4518145 - Bundle Light posto di lavoro/client aggiuntivo','DataType':'integer','MinValue':1,'MaxValue':15,'DefaultValue':1,'Tag':4518145}"),
                new InputItem("{'Key':'postazioniLavoroAccresco','Text':'4528145 - Bundle Full posto di lavoro/client aggiuntivo','DataType':'integer','MinValue':1,'MaxValue':15,'DefaultValue':1,'Tag':4528145}"),
            }));
            a.DrawPage = _DrawPage;
            a.AllowNoChoice = true;
            Branch b1 = a.CreateBranchTo("upgradeAccrescoXML");
        }

        private void _AddActivity_UpgradeXML(Workflow wf)
        {
            Activity a = wf.CreateActivity("upgradeAccrescoXML");
            a.Title = "Vuoi attivare anche questo modulo? <span style='font-size:20px'>2 di 2</span>";
            a.TestoRiepilogo = "Modulo da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("4528026", "4528026 - Attivazione XML","4528026"),
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
                 new InputItem("{'Key':'uploadFile','Text':'Caricare un file PDF','DataType':'blob', 'Tag':'Blob'}")
            }));
            a.DrawPage = _DrawPage;

            //Branch b1 = a.CreateBranchTo("note");
            Branch b1 = a.CreateBranchToSummary();
        }

        private void _AddActivity_AddNote(Workflow wf)
        {
            Activity a = wf.CreateActivity("note");
            a.Title = "Aggiungi delle note";
            a.TestoRiepilogo = "Note:";
            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
                 new InputItem("{'Key':'noteWizard','Text':'Note','DataType':'text', 'Tag':'noteWizard'}")
            }));
            a.DrawPage = _DrawPage;
            a.AllowNoChoice = true;

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
