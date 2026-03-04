using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowDocs : Workflow
    {
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

        public WorkflowDocs(string key, string title, Action<StateContext> drawPage) : base(key, title)
        {
            _DrawPage = drawPage;

            List<string> activities = GetActivities(typeof(WorkflowDocs));

            foreach (string a in activities)
            {
                MethodInfo m = this.GetType().GetMethod(a, BindingFlags.NonPublic | BindingFlags.Instance);
                m.Invoke(this, new object[] { this });
            }
        }

        private void _AddActivity_TipoSoggetto(Workflow wf)
        {
            Activity a = wf.CreateActivity("sogg");
            a.Title = "Quale tipo di soggetto vuoi abilitare?";
            a.TestoRiepilogo = "Tipo di soggetto:";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("prof", "Professionista"),
                new InputItem("azi", "Azienda"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("docsCOMM");
            b1.Condition.IfOutputContainsItem("prof");

            Branch b2 = a.CreateBranchTo("docsAZI");
            b2.Condition.IfOutputContainsItem("azi");
        }

        private void _AddActivity_DocsAzi(Workflow wf)
        {
            Activity a = wf.CreateActivity("docsAZI");
            a.Title = "La procedura non è ancora disponibile<br/>per questa tipologia di cliente.";
            a.DrawPage = _DrawPage;
            a.AllowNoChoice = true;
        }

        private void _AddActivity_ArchCondPromo(Workflow wf)
        {
            Activity a = wf.CreateActivity("docsCOMM");
            a.Title = "Scegli il tipo di archiviazione:";
            a.TestoRiepilogo = "Tipo di archiviazione:";

            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("ArchDLSemplificazioni", "Archiviazione DL Semplificazioni", 0, false, false, true),
                new InputItem("ArchCondFULL", "Archiviazione e Condivisione Full", 0, false, false, true),
                new InputItem("ArchCondFullPromo", "Archiviazione e Condivisione Full <span style='background-color:yellow'>PROMO</span>", true, false),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("TipoArchCondPromo");
            b1.Condition.IfOutputContainsItem("ArchCondFullPromo");
        }

        private void _AddActivity_TipoArchCondPromo(Workflow wf)
        {
            Activity a = wf.CreateActivity("TipoArchCondPromo");
            a.Title = "Scegli il tipo di archiviazione:";
            a.TestoRiepilogo = "Tipo di archiviazione:";

            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("DOCS.SP011","DOCS.SP011 - Condiviso per Studi FULL PROMO - 1PDL","DOCS.SP011"),
                new InputItem("DOCS.SP013","DOCS.SP013 - Condiviso per Studi FULL PROMO - fino a 3 PDL","DOCS.SP013"),
                new InputItem("DOCS.SP015","DOCS.SP015 - Condiviso per Studi FULL PROMO - fino a 5 PDL","DOCS.SP015"),
                new InputItem("DOCS.SP110","DOCS.SP110 - Condiviso per Studi FULL PROMO - fino a 10 PDL","DOCS.SP110"),
                new InputItem("DOCS.SP115","DOCS.SP115 - Condiviso per Studi FULL PROMO - fino a 15 PDL","DOCS.SP115"),
                new InputItem("DOCS.SP120","DOCS.SP120 - Condiviso per Studi FULL PROMO - fino a 20 PDL","DOCS.SP120"),
                new InputItem("DOCS.SP199","DOCS.SP199 - Condiviso per Studi FULL PROMO - oltre 20 PDL","DOCS.SP199")
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
