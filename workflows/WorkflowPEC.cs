using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowPEC : Workflow
    {
        //public bool possiedeContratto { get; set; }

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


        public WorkflowPEC(string key, string title, Action<StateContext> drawPage/*, bool possiedeContratto*/) : base(key, title)
        {
            _DrawPage = drawPage;

            //this.possiedeContratto = possiedeContratto;

            List<string> methods = ShowMethods(typeof(WorkflowPEC));

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

            Branch b1 = null;
            b1 = a.CreateBranchTo("tipoContratto"); 

            //if (possiedeContratto)
            //{
            //    b1 = a.CreateBranchTo("upgrade");
            //}
            //else
            //{
            //    b1 = a.CreateBranchTo("nuovoContratto");
            //}
        }

        private void _AddActivity_TipoContratto(Workflow wf)
        {
            Activity a = wf.CreateActivity("tipoContratto");
            a.Title = "Che tipo di contratto desideri attivare?";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
               new InputItem("nContr", "Modulo base"),
               new InputItem("upgrade", "Upgrade"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("nuovoContratto");
            b1.Condition.IfOutputContainsItem("nContr");

            Branch b2 = a.CreateBranchTo("upgradePEC");
            b2.Condition.IfOutputContainsItem("upgrade");
        }

        private void _AddActivity_AttivaNuovoContratto(Workflow wf)
        {
            Activity a = wf.CreateActivity("nuovoContratto");
            a.Title = "Vuoi creare un nuovo contratto?";
            a.TestoRiepilogo = "Nuovo contratto:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("3208029", "3208029 - 20 caselle con 22 accessi utenti", "3208029", true)
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_Upgrade(Workflow wf)
        {
            Activity a = wf.CreateActivity("upgradePEC");
            a.Title = "Vuoi effettuare l'upgrade del contratto con quale modulo?";
            a.TestoRiepilogo = "Upgrade del contratto:";
            //a.Description = "Breve descrizione...";
            //a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
            //    new InputItem("3008219", "3008219 - Ulteriori 10 caselle con 1 operatore e 10 accessi esterni","3008219"),
            //    new InputItem("3008229", "3008229 - Ulteriori 20 caselle con 1 operatore e 20 accessi esterni", "3008229"),
            //    new InputItem("3008259", "3008259 - Ulteriori 50 caselle con 2 operatore e 50 accessi esterni ", "3008259"),
            //}));

            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
                new InputItem("{'Key':'upgradePEC','Text':'3008219 - Ulteriori 10 caselle con 1 operatore e 10 accessi esterni','DataType':'integer','MinValue':1,'MaxValue':5,'DefaultValue':1, 'Tag':'3008219'}"),
                new InputItem("{'Key':'upgradePEC','Text':'3008229 - Ulteriori 20 caselle con 1 operatore e 20 accessi esterni','DataType':'integer','MinValue':1,'MaxValue':5,'DefaultValue':1, 'Tag':'3008229'}"),
                new InputItem("{'Key':'upgradePEC','Text':'3000259 - Ulteriori 50 caselle con 2 operatore e 50 accessi esterni','DataType':'integer','MinValue':1,'MaxValue':5,'DefaultValue':1,'Tag':'3000259'}"),
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