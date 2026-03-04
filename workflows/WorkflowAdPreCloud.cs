using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowAdPreCloud : Workflow
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


        public WorkflowAdPreCloud(string key, string title, Action<StateContext> drawPage) : base(key, title)
        {
            _DrawPage = drawPage;

            List<string> methods = ShowMethods(typeof(WorkflowAdPreCloud));

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
            a.Title = "Quale configurazione vuoi attivare?";
            a.TestoRiepilogo = "Configurazione da attivare";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("singoli", "Moduli singoli"),
                new InputItem("bundle", "Bundle"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("singoli");
            b1.Condition.IfOutputContainsItem("singoli");

            Branch b2 = a.CreateBranchTo("bundle");
            b2.Condition.IfOutputContainsItem("bundle");
        }
        private void _AddActivity_PrecSingoli(Workflow wf)
        {
            Activity a = wf.CreateActivity("singoli");
            a.Title = "Quale modulo vuoi attivare?";
            a.TestoRiepilogo = "Modulo da attivare";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                new InputItem("6008363", "6008363 - Prelievo ISA precompilato da Cassetto Fiscale", "6008363"),
                new InputItem("6008353", "6008353 - Bolli su FE", "6008353"),
                new InputItem("6008393", "6008393 - Prelievo CU da Cassetto Fiscale", "6008393"),
                new InputItem("6008373", "6008373 - Prelievo Corrispettivi Telematici", "6008373"),
				new InputItem("6008303", "6008303 - Prelievo Precompilata PF", "6008303"),
			}));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }
        
        private void _AddActivity_PrecBundle(Workflow wf)
        {
            Activity a = wf.CreateActivity("bundle");
            a.Title = "Quale modulo vuoi attivare?";
            a.TestoRiepilogo = "Modulo da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                //new InputItem("6008289", "6008289 - Bundle Adempimenti Precompilati", "6008289"),
                //new InputItem("6008313", "6008313 - Recupero automatico Fatture da Portale Fatture&Corrispettivi AdE", "6008313"),
                new InputItem("6008399", "6008399 - Bundle Adempimenti Precompilati 2025", "6008399"),
			}));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        //private void _AddActivity_RecuperoAutomatico(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("recuperoAutomatico");
        //    a.Title = "Desideri attivare il seguente modulo?";
        //    a.Title = "Modulo da attivare:";
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //        new InputItem("6008313", "6008313 - Recupero automatico Fatture da Portale Fatture&Corrispettivi AdE", "6008313"),
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