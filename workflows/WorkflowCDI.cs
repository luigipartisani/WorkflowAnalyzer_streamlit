using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowCDI : Workflow
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

        public WorkflowCDI(string key, string title, Action<StateContext> drawPage, int tipoLicenza) : base(key, title)
        {
            _DrawPage = drawPage;

            List<string> methods = ShowMethods(typeof(WorkflowCDI));

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
            //a.Description = "Breve descrizione...";
            //a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
            //    //new InputItem("demo", "Demo"),
            //    //new InputItem("stan", "Standard"),
            //     new InputItem("{'Key':'tipoLicenza','Text':'Demo','DataType':'radioText', 'Tag':'tipoLicenza','Style':'visible','Index':0}"),
            //      new InputItem("{'Key':'tipoLicenza','Text':'Standard','DataType':'radioText', 'Tag':'tipoLicenza', 'Style':'hidden','Index':1}"),
            //}));
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
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("prof", "Professionista", "PDS.COMM"),
                new InputItem("azi", "Azienda", "PDS.AZI"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("azienda");
        }

        //private void _AddActivity_Versione(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("ver");
        //    a.Title = "Quale configurazione desideri attivare?";
        //    a.TestoRiepilogo = "Configurazione da attivare:";
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //        new InputItem("azienda", "Azienda"),
        //        new InputItem("postiLavoro", "Posti di lavoro")
        //    }));
        //    a.DrawPage = _DrawPage;

        //    Branch b1 = a.CreateBranchTo("azienda");
        //    b1.Condition.IfOutputContainsItem("azienda");

        //    Branch b2 = a.CreateBranchTo("postiLavoro");
        //    b2.Condition.IfOutputContainsItem("postiLavoro");
        //}

        private void _AddActivity_AttivaAzienda(Workflow wf)
        {
            Activity a = wf.CreateActivity("azienda");
            a.Title = "Quale versione desideri attivare?";
            a.TestoRiepilogo = "Versione da attivare:";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("4408053", "4408053 - Fino a 5 aziende ", "4408053"),
                new InputItem("4408103", "4408103 - Fino a 10 aziende", "4408103"),
                new InputItem("4408253", "4408253 - Fino a 25 aziende", "4408253"),
                new InputItem("4408503", "4408503 - Fino a 50 aziende", "4408503"),
                new InputItem("4408903", "4408903 - Illimitate ", "4408903")
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        //private void _AddActivity_AttivaPdl(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("postiLavoro");
        //    a.Title = "Quale versione desideri attivare?";
        //    a.TestoRiepilogo = "Versione da attivare:";
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //        new InputItem("4408953", "4408953 - Fino a 5 posti di lavoro aziende illimitate ", "4408953"),
        //        new InputItem("4408963", "4408963 - Oltre a 5 posti di lavoro aziende illimitate", "4408963")
        //    }));
        //    a.DrawPage = _DrawPage;

        //    Branch b1 = a.CreateBranchTo("uploadFile");
        //}

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