using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowStudiok : Workflow
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

        public WorkflowStudiok(string key, string title, Action<StateContext> drawPage/*, bool possiedeContratto*/) : base(key, title)
        {
            _DrawPage = drawPage;

            List<string> activities = GetActivities(typeof(WorkflowStudiok));

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

            Branch b1 = a.CreateBranchTo("tipoParcellazione");

            //Branch b1 = null;

            //if (!possiedeContratto)
            //    b1 = a.CreateBranchTo("nuovoStudiok");
            //else
            //    b1 = a.CreateBranchTo("upgradeStudiok");
        }

        private void _AddActivity_TipoParcellazione(Workflow wf)
        {
            Activity a = wf.CreateActivity("tipoParcellazione");
            a.Title = "Quali moduli desideri attivare?";
            a.TestoRiepilogo = "Moduli da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
               new InputItem("parcPrevContratti", "Parcellazione, Fatturazione, Preventivi e Contratti","4008046"),
               new InputItem("parcContratti", "Parcellazione Fatturazione","4008016"),
               new InputItem("gestStudio", "Gestione Studio"),
               new InputItem("upgrade", "Upgrade")
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("nuovoStudiok");
            b1.Condition.IfOutputContainsItem("gestStudio");

            Branch b2 = a.CreateBranchTo("upgradeStudiok");
            b2.Condition.IfOutputOfActivityContainsItem("lic","Standard").And.IfOutputContainsItem("upgrade");

            Branch b3 = a.CreateBranchTo("upgradeStudiokDEMO");
            b3.Condition.IfOutputOfActivityContainsItem("lic", "Demo").And.IfOutputContainsItem("upgrade");

            Branch b4 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_NumeroPostazioni(Workflow wf)
        {
            Activity a = wf.CreateActivity("nuovoStudiok");
            a.Title = "Di quanti client/accessi necessita?";
            a.TestoRiepilogo = "Client/accessi";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
               //new InputItem("4008015", "4008015 - StudiOK Gestione Studio - Workstation","4008015"),
               //new InputItem("4008055", "4008055 - StudiOK Gestione Studio - 5 accessi","4008055"),
               //new InputItem("4000105", "4000105 - StudiOK Gestione Studio - 10 accessi","4000105"),
               //new InputItem("4008155", "4008155 - StudiOK Gestione Studio - 15 accessi","4008155"),
               //new InputItem("4008145", "4008145 - StudiOK Gestione Studio - ulteriore client/accesso Oltre i 15 posti di lavoro","4008145"),

               new InputItem("{'Key':'postazioniStudioK','Text':'Gestione Studio client/accessi','DataType':'integer','MinValue':1,'MaxValue':15,'DefaultValue':1,'Tag':'xStudiok'}"),
               //new InputItem("{'Key':'postazioniStudioK','Text':'Gestione Studio accesso aggiuntivo (oltre 15 accessi)','DataType':'integer','MinValue':1,'MaxValue':100,'DefaultValue':1,'Tag':'xStudiok'}"),
               new InputItem("{'Key':'postazioniStudioK','Text':'4008145 - Oltre i 15 posti di lavoro','DataType':'integer','MinValue':1,'MaxValue':100,'DefaultValue':1,'Tag':'xStudiok'}")

            }));
            a.DrawPage = _DrawPage;

            //Branch b1 = a.CreateBranchTo("nuovoModuli");

            Branch b1 = a.CreateBranchTo("upgradeStudiok");
            b1.Condition.IfOutputOfActivityContainsItem("tipoParcellazione", "upgrade").And.IfOutputOfActivityContainsItem("lic", "Standard");

            Branch b2 = a.CreateBranchTo("upgradeStudiokDEMO");
            b2.Condition.IfOutputContainsItem("tipoParcellazione").And.IfOutputOfActivityContainsItem("lic", "Demo");

            Branch b3 = a.CreateBranchTo("uploadFile");

        }

        private void _AddActivity_ModuliNuovo(Workflow wf)
        {
            Activity a = wf.CreateActivity("nuovoModuli");
            a.Title = "Quali moduli desidera attivare?";
            a.TestoRiepilogo = "Moduli da attivare";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
               new InputItem("4008046", "4008046 - Parcellazione, fatturazione, preventivi e contratti","4008046"),
               new InputItem("4008016", "4008016 - StudiOK Parcellazione/Fatturazione","4008016")
            }));
            a.DrawPage = _DrawPage;
            a.AllowNoChoice = true;

            Branch b1 = a.CreateBranchTo("nuovoModuliUpgrade");
            b1.Condition.IfOutputContainsItem("4008046").Or.IfOutputContainsItem("4008016");

            Branch b2 = a.CreateBranchTo("upgradeStudiok");
            b2.Condition.IfOutputOfActivityContainsItem("tipoParcellazione", "upgrade").And.IfOutputOfActivityContainsItem("lic", "Standard");

            Branch b3 = a.CreateBranchTo("upgradeStudiokDEMO");
            b3.Condition.IfOutputOfActivityContainsItem("tipoParcellazione", "upgrade").And.IfOutputOfActivityContainsItem("lic", "Demo");


            Branch b4 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_ModuliNuovoUpgrade(Workflow wf)
        {
            Activity a = wf.CreateActivity("nuovoModuliUpgrade");
            a.Title = "Quale upgrade vuoi attivare?";
            a.TestoRiepilogo = "Upgrade da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
               new InputItem("4008056", "4008056 - Upgrade Preventivi e contratti","4008056")
            }));
            a.DrawPage = _DrawPage;
            a.AllowNoChoice = true;

            Branch b1 = a.CreateBranchTo("uploadFile");

        }

        private void _AddActivity_UpgradeStudiok(Workflow wf)
        {
            Activity a = wf.CreateActivity("upgradeStudiok");
            a.Title = "Quale upgrade vuoi attivare?";
            a.TestoRiepilogo = "Upgrade da attivare:";
            //a.Description = "Breve descrizione...";
            //a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
            //   new InputItem("4008056", "4008056 - Upgrade Preventivi e contratti","4008056"),
            //   new InputItem("4008145", "4008145 - Ulteriore client aggiuntivo specificare il numero accessi aggiuntivi","4008145")
            //}));

            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
               new InputItem("{'Key':'postazioniStudioK','Text':'4008056 - Upgrade Preventivi e contratti','DataType':'integer','MinValue':1,'MaxValue':1,'DefaultValue':1,'Tag':'4008056'}"),
               new InputItem("{'Key':'postazioniStudioK','Text':'4008145 - Oltre i 15 posti di lavoro','DataType':'integer','MinValue':1,'MaxValue':15,'DefaultValue':1,'Tag':'4008145'}"),
            }));

            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_UpgradeStudiokDemo(Workflow wf)
        {
            Activity a = wf.CreateActivity("upgradeStudiokDEMO");
            a.Title = "Quale upgrade DEMO vuoi attivare?";
            a.TestoRiepilogo = "Upgrade da attivare:";
            //a.Description = "Breve descrizione...";
            //a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
            //   new InputItem("4008056", "4008056 - Upgrade Preventivi e contratti","4008056"),
            //   new InputItem("4008145", "4008145 - Ulteriore client aggiuntivo specificare il numero accessi aggiuntivi","4008145")
            //}));

            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
               new InputItem("{'Key':'postazioniStudioK','Text':'4008056 - Upgrade Preventivi e contratti','DataType':'integer','MinValue':1,'MaxValue':1,'DefaultValue':1,'Tag':'4008056'}"),
            }));

            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }



        private void _AddActivity_UploadPDF(Workflow wf)
        {
            Activity a = wf.CreateActivity("uploadFile");
            a.Title = "Carica il PDF del contratto";
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
