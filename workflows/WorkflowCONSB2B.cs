using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowCONSB2B : Workflow
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

        public WorkflowCONSB2B(string key, string title, Action<StateContext> drawPage) : base(key, title)
        {
            _DrawPage = drawPage;

            List<string> activities = GetActivities(typeof(WorkflowCONSB2B));

            foreach (string a in activities)
            {
                MethodInfo m = this.GetType().GetMethod(a, BindingFlags.NonPublic | BindingFlags.Instance);
                m.Invoke(this, new object[] { this });
            }
        }

        //        private void _AddActivity_TipoLicenza(Workflow wf)
        //        {
        //            Activity a = wf.CreateActivity("lic");
        //            a.Title = "Che tipo di licenza desideri attivare?";
        //            a.TestoRiepilogo = "Tipo di licenza da attivare:";
        //            //a.Description = "Breve descrizione...";
        //            //a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
        //            //     new InputItem("{'Key':'tipoLicenza','Text':'Demo','DataType':'radioText', 'Tag':'tipoLicenza','Style':'visible','Index':0}"),
        //            //      new InputItem("{'Key':'tipoLicenza','Text':'Standard','DataType':'radioText', 'Tag':'tipoLicenza', 'Style':'hidden','Index':1}"),
        //            //}));
        //            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[]
        //{
        //                new InputItem("demo", "Demo - fino al " + DateTime.Now.AddDays(7).ToShortDateString()),
        //                new InputItem("standard","Standard")
        //}));
        //            a.DrawPage = _DrawPage;

        //            Branch b1 = a.CreateBranchTo("fatture");
        //        }

        private void _AddActivity_Scelta(Workflow wf)
        {
            Activity a = wf.CreateActivity("scelta");
            a.Title = "Quale modulo desideri attivare?";
            a.TestoRiepilogo = "Modulo da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("consDigB2B","Conservazione digitale B2B")
                //new InputItem("mantCons","Mantenimento conservazione"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("fatture");
            b1.Condition.IfOutputContainsItem("consDigB2B");

            //Branch b2 = a.CreateBranchTo("ulterioriServizi");
            //b2.Condition.IfOutputContainsItem("mantCons");
        }

        private void _AddActivity_Fatture(Workflow wf)
        {
            Activity a = wf.CreateActivity("fatture");
            a.Title = "Quante fatture desideri attivare?";
            a.TestoRiepilogo = "Fatture da attivare:";
            //a.Description = "Breve descrizione...";
            //a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
            //    new InputItem("6518023","6518023 - Fino a 2.000 ft b2b attive e passive ","6518023"),
            //    new InputItem("6518053","6518053 - Fino a 5.000 ft b2b attive e passive ","6518053"),
            //    new InputItem("6518103","6518103 - Fino a 10.000 ft b2b attive e passive ","6518103"),
            //    new InputItem("6518993","6518993 - Conservazione Digitale Online Fatture Elettroniche B2B a consuntivo ","6518993"),
            //}));
            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
                new InputItem("{'Key':'fattureCSB2B','Text':'6518023 - Fino a 2.000 ft b2b attive e passive ','DataType':'integer','MinValue':1,'MaxValue':10,'DefaultValue':1, 'Tag':'6518023'}"),
                new InputItem("{'Key':'fattureCSB2B','Text':'6518053 - Fino a 5.000 ft b2b attive e passive ','DataType':'integer','MinValue':1,'MaxValue':10,'DefaultValue':1, 'Tag':'6518053'}"),
                new InputItem("{'Key':'fattureCSB2B','Text':'6518103 - Fino a 10.000 ft b2b attive e passive','DataType':'integer','MinValue':1,'MaxValue':10,'DefaultValue':1,'Tag':'6518103'}"),
                new InputItem("{'Key':'fattureCSB2B','Text':'6518993 - Conservazione Digitale Online Fatture Elettroniche B2B a consuntivo','DataType':'integer','MinValue':1,'MaxValue':1,'DefaultValue':1,'Tag':'6518993'}"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        //private void _AddActivity_UlterioriServizi(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("ulterioriServizi");
        //    a.Title = "Desideri attivare questo modulo?";
        //    a.TestoRiepilogo = "Attivare il modulo:";
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //        new InputItem("910","910 - Mantenimento in Conservazione Sostitutiva dei dati e loro consultazione in caso di disdetta del Servizio ","910"),
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
