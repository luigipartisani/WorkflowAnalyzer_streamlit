using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowCDO : Workflow
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

        public WorkflowCDO(string key, string title, Action<StateContext> drawPage) : base(key, title)
        {
            _DrawPage = drawPage;

            List<string> activities = GetActivities(typeof(WorkflowCDO));

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
        //    a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
        //         new InputItem("{'Key':'tipoLicenza','Text':'Demo','DataType':'radioText', 'Tag':'tipoLicenza','Style':'hidden','Index':0}"),
        //          new InputItem("{'Key':'tipoLicenza','Text':'Standard','DataType':'radioText', 'Tag':'tipoLicenza', 'Style':'hidden','Index':1}"),
        //    }));
        //    a.DrawPage = _DrawPage;

        //    Branch b1 = a.CreateBranchTo("adFiscCont");
        //}

        //private void _AddActivity_TipoConservazione(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("tipoModulo");
        //    a.Title = "Che tipo di conservazione desideri attivare?";
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //        new InputItem("adFiscCont","Adempimenti fiscali e contabili"),
        //        new InputItem("fatture","Fatture"),
        //    }));
        //    a.DrawPage = _DrawPage;

        //    Branch b1 = a.CreateBranchTo("adFiscCont");
        //    b1.Condition.IfOutputContainsItem("adFiscCont");

        //    Branch b2 = a.CreateBranchTo("fatture");
        //    b2.Condition.IfOutputContainsItem("fatture");
        //}

        private void _AddActivity_TipoAttivazione(Workflow wf)
        {
            Activity a = wf.CreateActivity("tipoAttivazione");
            a.Title = "Tipo di attivazione";
            a.TestoRiepilogo = "Tipo di attivazione:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("nuova","Nuova attivazione"),
                new InputItem("upgrade","Upgrade")
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("adFiscCont");
            b1.Condition.IfOutputContainsItem("nuova");

            Branch b2 = a.CreateBranchTo("adFiscContUpgrade");
            b2.Condition.IfOutputContainsItem("upgrade");
        }

        private void _AddActivity_AdempimentiFiscali(Workflow wf)
        {
            Activity a = wf.CreateActivity("adFiscCont");
            a.Title = "Quante versione desideri attivare?";
            a.TestoRiepilogo = "Versione da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                new InputItem("6500001","6500001 - Conservazione Digitale Online - Attivazione una tantum","6500001;CONS.GATEWAY", true, true),
                new InputItem("6508059","6508059 - Conservazione digitale online fino a 5.000 pagine","6508059"),
                new InputItem("6508089","6508089 - Conservazione digitale online fino a 8.000 pagine","6508089"),
                new InputItem("6508109","6508109 - Conservazione digitale online fino a 10.000 pagine","6508109"),
                new InputItem("6508159","6508159 - Conservazione digitale online fino a 15.000 pagine","6508159"),
                new InputItem("6508209","6508209 - Conservazione digitale online fino a 20.000 pagine","6508209"),
                new InputItem("6508309","6508309 - Conservazione digitale online fino a 30.000 pagine","6508309"),
                new InputItem("6508409","6508409 - Conservazione digitale online fino a 40.000 pagine","6508409"),
                new InputItem("6508509","6508509 - Conservazione digitale online fino a 50.000 pagine","6508509"),
                new InputItem("6508609","6508609 - Conservazione digitale online fino a 60.000 pagine","6508609"),
                new InputItem("6508709","6508709 - Conservazione digitale online fino a 70.000 pagine","6508709"),
                new InputItem("6508809","6508809 - Conservazione digitale online fino a 80.000 pagine","6508809"),
                new InputItem("6508909","6508909 - Conservazione digitale online fino a 90.000 pagine","6508909"),
                new InputItem("6508119","6508119 - Conservazione digitale online fino a 100.000 pagine","6508119")
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_AdempimentiFiscaliUpgrade(Workflow wf)
        {
            Activity a = wf.CreateActivity("adFiscContUpgrade");
            a.Title = "Quante versione desideri attivare?";
            a.TestoRiepilogo = "Versione da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                new InputItem("6508059","6508059 - Conservazione digitale online fino a 5.000 pagine","6508059"),
                new InputItem("6508089","6508089 - Conservazione digitale online fino a 8.000 pagine","6508089"),
                new InputItem("6508109","6508109 - Conservazione digitale online fino a 10.000 pagine","6508109"),
                new InputItem("6508159","6508159 - Conservazione digitale online fino a 15.000 pagine","6508159"),
                new InputItem("6508209","6508209 - Conservazione digitale online fino a 20.000 pagine","6508209"),
                new InputItem("6508309","6508309 - Conservazione digitale online fino a 30.000 pagine","6508309"),
                new InputItem("6508409","6508409 - Conservazione digitale online fino a 40.000 pagine","6508409"),
                new InputItem("6508509","6508509 - Conservazione digitale online fino a 50.000 pagine","6508509"),
                new InputItem("6508609","6508609 - Conservazione digitale online fino a 60.000 pagine","6508609"),
                new InputItem("6508709","6508709 - Conservazione digitale online fino a 70.000 pagine","6508709"),
                new InputItem("6508809","6508809 - Conservazione digitale online fino a 80.000 pagine","6508809"),
                new InputItem("6508909","6508909 - Conservazione digitale online fino a 90.000 pagine","6508909"),
                new InputItem("6508119","6508119 - Conservazione digitale online fino a 100.000 pagine","6508119")
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        //private void _AddActivity_Fatture(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("fatture");
        //    a.Title = "Che tipo di conservazione desideri attivare?";
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //        new InputItem("6518023","6518023 - Fino a 2.000 ft b2b attive e passive ","6518023"),
        //        new InputItem("6518053","6518053 - Fino a 5.000 ft b2b attive e passive ","6518053"),
        //        new InputItem("6518103","6518103 - Fino a 10.000 ft b2b attive e passive ","6518103"),
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
            a.DrawPage = _DrawPage;
        }

    }
}
