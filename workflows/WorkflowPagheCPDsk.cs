using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowPagheCPDsk : Workflow
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

        public WorkflowPagheCPDsk(string key, string title, Action<StateContext> drawPage) : base(key, title)
        {
            _DrawPage = drawPage;

            List<string> activities = GetActivities(typeof(WorkflowPagheCPDsk));

            foreach (string a in activities)
            {
                MethodInfo m = this.GetType().GetMethod(a, BindingFlags.NonPublic | BindingFlags.Instance);
                m.Invoke(this, new object[] { this });
            }
        }

        //private void _AddActivity_TipoLicenza(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("lic");
        //    a.Title = "Che tipo di licenza desideri attivare?";
        //    a.TestoRiepilogo = "Tipo di licenza da attivare:";
        //    //a.Description = "Breve descrizione...";
        //    //a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
        //    //     new InputItem("{'Key':'tipoLicenza','Text':'Demo','DataType':'radioText', 'Tag':'tipoLicenza','Style':'hidden','Index':0}"),
        //    //      new InputItem("{'Key':'tipoLicenza','Text':'Standard','DataType':'radioText', 'Tag':'tipoLicenza', 'Style':'hidden','Index':1}"),
        //    //}));
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //         new InputItem("Demo","Demo"),
        //          new InputItem("Standard", "Standard"),
        //    }));
        //    a.DrawPage = _DrawPage;

        //    //Branch b1 = a.CreateBranchTo("tipoContratto");
        //    Branch b1 = a.CreateBranchTo("tipoContratto");
        //    b1.Condition.IfOutputContainsItem("Standard");

        //    Branch b2 = a.CreateBranchTo("bundleDemo");
        //    b2.Condition.IfOutputContainsItem("Demo");
        //}

        private void _AddActivity_TipoContratto(Workflow wf)
        {
            Activity a = wf.CreateActivity("tipoContratto");
            a.Title = "Che tipo di contratto vuoi attivare?";
            a.TestoRiepilogo = "Tipo di contratto da attivare:";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
               new InputItem("bundle", "Bundle"),
               new InputItem("modAgg", "Moduli aggiuntivi"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("bundle");
            b1.Condition.IfOutputContainsItem("bundle");

            Branch b2 = a.CreateBranchTo("moduliAgg");
            b2.Condition.IfOutputContainsItem("modAgg");
        }

        private void _AddActivity_NuovoContratto(Workflow wf)
        {
            Activity a = wf.CreateActivity("bundle");
            a.Title = "Quale modulo vuoi attivare?";
            a.TestoRiepilogo = "Modulo da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("3198306","3198306 - Bundle fino a 300 cedolini/anno","3198306"),
                new InputItem("3198066","3198066 - Bundle fino a 600 cedolini/anno", "3198066"),
                new InputItem("3198126","3198126 - Bundle fino a 1.200 cedolini/anno", "3198126"),
                new InputItem("3198026","3198026 - Bundle fino a 2.400 cedolini/anno", "3198026"),
                new InputItem("3198136","3198136 - Bundle fino a 3.600 cedolini/anno", "3198136"),
                new InputItem("3198146","3198146 - Bundle fino a 4.800 cedolini/anno", "3198146"),
                new InputItem("3198166","3198166 - Bundle fino a 6.000 cedolini/anno", "3198166"),
                new InputItem("3198726","3198726 - Bundle fino a 7.200 cedolini/anno", "3198726"),
                new InputItem("3198846","3198846 - Bundle fino a 8.400 cedolini/anno", "3198846"),
                new InputItem("3198966","3198966 - Bundle fino a 9.600 cedolini/anno", "3198966"),
                new InputItem("3198106","3198106 - Bundle fino a 10.800 cedolini/anno","3198106"),
                new InputItem("3198226","3198226 - Bundle fino a 12.000 cedolini/anno","3198226")
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_NuovoContrattoDemo(Workflow wf)
        {
            Activity a = wf.CreateActivity("moduliAgg");
            a.Title = "Quali moduli aggiuntivi desideri attivare? <span style='font-size:20px'>1 di 2</span>";
            a.TestoRiepilogo = "Modulo da attivare:";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                new InputItem("319001", "319001 - GigaDoc invio dei cedolini ai dipendenti per e-mail no indicatori","319001"),
                new InputItem("330001", "330001 - Certificazione Unica Full", "330001"),
                new InputItem("338009", "338009 - Rileva Light WEB no indicatori", "338009")
            }));
            a.AllowNoChoice = true;
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("moduliAgg2");
        }

        private void _AddActivity_Upgrade(Workflow wf)
        {
            Activity a = wf.CreateActivity("moduliAgg2");
            a.Title = "Quali moduli aggiuntivi desideri attivare? <span style='font-size:20px'>2 di 2</span>";
            a.TestoRiepilogo = "Modulo da attivare:";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                new InputItem("3198307","3198307 - ANF fino a 300 cedolini/anno",      "3198307"),
                new InputItem("3198067","3198067 - ANF fino a 600 cedolini/anno",     "3198067"),
                new InputItem("3198217","3198217 - ANF fino a 1.200 cedolini/anno",   "3198217"),
                new InputItem("3198027","3198027 - ANF fino a 2.400 cedolini/anno",   "3198027"),
                new InputItem("3198137","3198137 - ANF fino a 3.600 cedolini/anno",   "3198137"),
                new InputItem("3198147","3198147 - ANF fino a 4.800 cedolini/anno",   "3198147"),
                new InputItem("3198167","3198167 - ANF fino a 6.000 cedolini/anno",   "3198167"),
                new InputItem("3198187","3198187 - ANF fino a 7.200 cedolini/anno",   "3198187"),
                new InputItem("3198847","3198847 - ANF fino a 8.400 cedolini/anno",   "3198847"),
                new InputItem("3198097","3198097 - ANF fino a 9.600 cedolini/anno",   "3198097"),
                new InputItem("3198107","3198107 - ANF fino a 10.800 cedolini/anno",  "3198107"),
                new InputItem("3198227","3198227 - ANF fino a 12.000 cedolini/anno",  "3198227")
            }));
            a.AllowNoChoice = true;
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