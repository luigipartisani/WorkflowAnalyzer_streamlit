using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowAnti : Workflow
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


        public WorkflowAnti(string key, string title, Action<StateContext> drawPage) : base(key, title)
        {
            _DrawPage = drawPage;

            List<string> methods = ShowMethods(typeof(WorkflowAnti));

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

            Branch b1 = a.CreateBranchTo("modulo");
        }

        private void _AddActivity_Modulo(Workflow wf)
        {
            Activity a = wf.CreateActivity("modulo");
            a.Title = "Che modulo desideri attivare?";
            a.Title = "Modulo da attivare";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("9208059", "9208059 - 1 soggetto operante fino a 500 anagrafiche", "9208059"),
                new InputItem("9208109", "9208109 - 1 soggetto operante, anagrafiche illimitate", "9208109"),
                new InputItem("9218059", "9218059 - da 2 a 5 soggetti operanti max 500 anagrafiche", "9218059"),
                new InputItem("9218109", "9218109 - da 2 a 5 soggetto operante Gestione illimitata di schede anagrafiche", "9218109"),
                new InputItem("9228059", "9228059 - da 6 a 15 soggetti operanti max 500 anagrafiche", "9228059"),
                new InputItem("9228109", "9228109 - da 6 a 15 soggetto operanti anagrafiche illimitate", "9228109"),
                new InputItem("9230809", "9230809 - Soggetti operante illimitati, anagrafiche illimitate", "9230809")
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