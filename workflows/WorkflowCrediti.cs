using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowCrediti : Workflow
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


        public WorkflowCrediti(string key, string title, Action<StateContext> drawPage, int tipoLicenza) : base(key, title)
        {
            _DrawPage = drawPage;

            List<string> methods = ShowMethods(typeof(WorkflowCrediti));

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
            switch (tipoLicenza)
            {
                case 0:
                    b1 = a.CreateBranchTo("sogg");
                    break;
                case 1:
                case 2:
                    b1 = a.CreateBranchTo("attivaModuloGCI");
                    break;
            }
        }

        private void _AddActivity_Soggetto(Workflow wf)
        {
            Activity a = wf.CreateActivity("sogg");
            a.Title = "Quale tipo di soggetto vuoi abilitare?";
            a.TestoRiepilogo = "Tipo di soggetto:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("prof", "Professionista", "PDS.COMM"),
                new InputItem("azi", "Azienda", "PDS.AZI"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("attivaModuloGCI"); 
        }

        private void _AddActivity_Modulo(Workflow wf)
        {
            Activity a = wf.CreateActivity("attivaModuloGCI");
            a.Title = "Vuoi attivare il modulo?";
            a.Title = "Modulo da attivare";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                //new InputItem("8808009", "8808009  - Utilizzo dei Crediti d'Imposta e gestione del SuperBonus Edilizia", "8808009", true)
                new InputItem("8808009", "8808009 - Utilizzo Crediti e Superbonus - Completo", "8808009"),
                new InputItem("8808019", "8808019 - Modulo Crediti e Contributi a Fondo Perduto", "8808019"),
                new InputItem("8808029", "8808029 - Modulo Bonus Edilizia - Superbonus", "8808029")
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