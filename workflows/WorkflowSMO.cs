using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowSMO : Workflow
    {
        private Action<StateContext> _DrawPage { get; set; }

        private int tipoLicenza { get; set; } // 0 -> niente, 1 -> comm, 2 -> azi

        private List<string> ShowMethods(Type type)
        {
            List<string> methods = new List<string>();

            foreach (var method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (method.Name.StartsWith("_AddActivity_")) methods.Add(method.Name);
            }

            return methods;
        }

        public WorkflowSMO(string key, string title, Action<StateContext> drawPage, int tipoLicenza) : base(key, title)
        {
            _DrawPage = drawPage;

            List<string> methods = ShowMethods(typeof(WorkflowSMO));
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
                    b1 = a.CreateBranchTo("tipoServizioCOMM");
                    break;
                case 2:
                    b1 = a.CreateBranchTo("tipoServizio");
                    break;
            }
        }

        private void _AddActivity_Soggetto(Workflow wf)
        {
            Activity a = wf.CreateActivity("sogg");
            a.Title = "Quale tipo di soggetto vuoi abilitare?";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("prof", "Professionista","9100104;PDS.COMM"),
                new InputItem("azi", "Azienda", "9100204;PDS.AZI"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("tipoServizio");
            b1.Condition.IfOutputContainsItem("azi");

            Branch b2 = a.CreateBranchTo("tipoServizioCOMM");
            b2.Condition.IfOutputContainsItem("prof");

        }

        private void _AddActivity_TipoServizioCOMM(Workflow wf)
        {
            Activity a = wf.CreateActivity("tipoServizioCOMM");
            a.Title = "Quale tipo di modulo vuoi abilitare?";
            a.TestoRiepilogo = "Tipo di modulo:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("9210101", "9210101 - Modulo Spese Mediche Online", "9210101"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }


        private void _AddActivity_TipoServizio(Workflow wf)
        {
            Activity a = wf.CreateActivity("tipoServizio");
            a.Title = "Quale tipo di modulo vuoi abilitare?";
            a.TestoRiepilogo = "Tipo di modulo:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("9210101", "9210101 - Modulo Spese Mediche Online", "9210101"),
                new InputItem("bundle", "Bundle Spese Mediche Online + Portale dei Servizi"),
                new InputItem("upgrade", "Upgrade per fatturazione elettronica B2B"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
            b1.Condition.IfOutputContainsItem("9210101");

            Branch b2 = a.CreateBranchTo("bundle");
            b2.Condition.IfOutputContainsItem("bundle");

            Branch b3 = a.CreateBranchTo("upgradeFatt");
            b3.Condition.IfOutputContainsItem("upgrade");
        }

        private void _AddActivity_Bundle(Workflow wf)
        {
            Activity a = wf.CreateActivity("bundle");
            a.Title = "Scegli il bundle";
            a.TestoRiepilogo = "Bundle scelto:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("2978022", "2978022 - Spese Mediche Online + Portale dei Servizi B2B con 400 fatture tramitate (attive e passive)","2978022"),
                new InputItem("2978042", "2978042 - Spese Mediche Online + Portale dei Servizi B2B con 600 fatture tramitate (attive e passive)","2978042"),
                new InputItem("2978102", "2978102 - Spese Mediche Online + Portale dei Servizi B2B con 1.200 fatture tramitate (attive e passive)","2978102"),
                new InputItem("2978202", "2978202 - Spese Mediche Online + Portale dei Servizi B2B con 2.200 fatture tramitate (attive e passive)","2978202"),
            }));
            a.DrawPage = _DrawPage;
            a.AllowNoChoice = true;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }


        private void _AddActivity_UpgradeFatture(Workflow wf)
        {
            Activity a = wf.CreateActivity("upgradeFatt");
            a.Title = "Vuoi aggiungere delle fatture?";
            a.TestoRiepilogo = "Fatture da aggiungere:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                new InputItem("2968022", "2968022 - Upgrade Portale dei Servizi B2B 400 Fatture tramitate","2968022"),
                new InputItem("2968042", "2968042 - Upgrade Portale dei Servizi B2B 600 Fatture tramitate","2968042"),
                new InputItem("2968102", "2968102 - Upgrade Portale dei Servizi B2B 1200 Fatture tramitate","2968102"),
                new InputItem("2968202", "2968202 - Upgrade Portale dei Servizi B2B 2200 Fatture tramitate","2968202"),
            }));
            a.DrawPage = _DrawPage;
            a.AllowNoChoice = true;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_UploadPDF(Workflow wf)
        {
            Activity a = wf.CreateActivity("uploadFile");
            a.Title = "Carica il PDF del contratto";
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