using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowCONSB2BITW : Workflow
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

        public WorkflowCONSB2BITW(string key, string title, Action<StateContext> drawPage) : base(key, title)
        {
            _DrawPage = drawPage;

            List<string> activities = GetActivities(typeof(WorkflowCONSB2BITW));

            foreach (string a in activities)
            {
                MethodInfo m = this.GetType().GetMethod(a, BindingFlags.NonPublic | BindingFlags.Instance);
                m.Invoke(this, new object[] { this });
            }
        }

        private void _AddActivity_Scelta(Workflow wf)
        {
            Activity a = wf.CreateActivity("scelta");
            a.Title = "Quale modulo desideri attivare?";
            a.TestoRiepilogo = "Modulo da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("consDigB2B","Conservazione digitale B2B"),
                new InputItem("mantCons","Mantenimento conservazione"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("fatture");
            b1.Condition.IfOutputContainsItem("consDigB2B");

            Branch b2 = a.CreateBranchTo("ulterioriServizi");
            b2.Condition.IfOutputContainsItem("mantCons");
        }

        private void _AddActivity_Fatture(Workflow wf)
        {
            Activity a = wf.CreateActivity("fatture");
            a.Title = "Quante fatture desideri attivare?";
            a.TestoRiepilogo = "Fatture da attivare:";
            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
                new InputItem("{'Key':'fattureCSB2B','Text':'6518993 - Conservazione Digitale Online Fatture Elettroniche B2B a consuntivo','DataType':'integer','MinValue':1,'MaxValue':1,'DefaultValue':1,'Tag':'6518993'}"),
                new InputItem("{'Key':'fattureCSB2B','Text':'6518103 - Fino a 10.000 ft b2b attive e passive','DataType':'integer','MinValue':1,'MaxValue':10,'DefaultValue':1,'Tag':'6518103'}")
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_UlterioriServizi(Workflow wf)
        {
            Activity a = wf.CreateActivity("ulterioriServizi");
            a.Title = "Desideri attivare questo modulo?";
            a.TestoRiepilogo = "Attivare il modulo:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("910","910 - Mantenimento in Conservazione Sostitutiva dei dati e loro consultazione in caso di disdetta del Servizio ","910"),
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
