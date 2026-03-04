using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowPDSRivSpecComm : Workflow
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


        public WorkflowPDSRivSpecComm(string key, string title, Action<StateContext> drawPage) : base(key, title)
        {
            _DrawPage = drawPage;

            List<string> methods = ShowMethods(typeof(WorkflowPDSRivSpecComm));

            foreach (string s in methods)
            {
                MethodInfo m = this.GetType().GetMethod(s, BindingFlags.NonPublic | BindingFlags.Instance);
                m.Invoke(this, new object[] { this });
            }
        }

        //private void _AddActivity_Modulo(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("modulo");
        //    a.Title = "Dati aggiuntivi";
        //    a.TestoRiepilogo = "Dati aggiuntivi";
        //    a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
        //         new InputItem("Postazione", "Codice Postazione"),
        //         new InputItem("Istituto", "Codice Istituto"),
        //         new InputItem("CUC", "Codice CUC"),
        //     }));
        //    a.DrawPage = _DrawPage;

        //    Branch b1 = a.CreateBranchToSummary();
        //}

        private void _AddActivity_DatiSHS(Workflow wf)
        {
            Activity a = wf.CreateActivity("datiSHS");
            a.Title = "Dati aggiuntivi";
            a.TestoRiepilogo = "Dati aggiuntivi:";
            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
                 new InputItem("{'Key':'datiSHS','Text':'Codice postazione','DataType':'text', 'Tag':'postazione'}"),
                 new InputItem("{'Key':'datiSHS','Text':'Codice istituto','DataType':'text', 'Tag':'istituto'}"),
                 new InputItem("{'Key':'datiSHS','Text':'Codice CUC','DataType':'text', 'Tag':'cuc'}"),
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
                 new InputItem("{'Key':'uploadFile','Text':'Carica PDF Delega Invio Firma','DataType':'blob', 'Tag':'Blob'}"),
                 new InputItem("{'Key':'uploadFile','Text':'Carica PDF Delega Conservazione','DataType':'blob', 'Tag':'Blob'}"),
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