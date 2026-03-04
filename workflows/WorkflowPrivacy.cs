using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowPrivacy : Workflow
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


        public WorkflowPrivacy(string key, string title, Action<StateContext> drawPage) : base(key, title)
        {
            _DrawPage = drawPage;

            List<string> methods = ShowMethods(typeof(WorkflowPrivacy));

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

            Branch b1 = a.CreateBranchTo("tipoServizio");
        }

        private void _AddActivity_Soggetto(Workflow wf)
        {
            Activity a = wf.CreateActivity("tipoServizio");
            a.Title = "Quale tipo di servizio vuoi attivare?";
            a.TestoRiepilogo = "Tipo di servizio:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("base", "Modulo base"),
                new InputItem("agg", "Servizi aggiuntivi"),
                new InputItem("upgrade", "Upgrade"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("base");
            b1.Condition.IfOutputContainsItem("base");

            Branch b2 = a.CreateBranchTo("agg");
            b2.Condition.IfOutputContainsItem("agg");

            Branch b3 = a.CreateBranchTo("upgradePEU");
            b3.Condition.IfOutputContainsItem("upgrade");
        }

        private void _AddActivity_ModuloBase(Workflow wf)
        {
            Activity a = wf.CreateActivity("base");
            a.Title = "Quale modulo desideri attivare?";
            a.TestoRiepilogo = "Modulo da attivare";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("9308519", "9308519  - 1 gestione privacy con 5 incaricati + Check Security 1 interrogazione", "9308519"),
                new InputItem("9308059", "9308059  - 5 gestioni privacy con 5 incaricati cad. + Check Security 1 interrogazione", "9308059"),
                new InputItem("9308209", "9308209  - 20 gestioni privacy con 5 incaricati cad. + Check Security 1 interrogazione", "9308209"),
                new InputItem("9308119", "9308119  - 50 gestioni privacy con 5 incaricati cad. + Check Security 1 interrogazione", "9308119")
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_ModuliAggiuntivi(Workflow wf)
        {
            Activity a = wf.CreateActivity("agg");
            a.Title = "Vuoi attivare servizi aggiuntivi?";
            a.TestoRiepilogo = "Moduli aggiuntivi da attivare";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("9338039", "9338039  - Check Security 3 interrogazioni", "9338039"),
                new InputItem("9338109", "9338109  - Check Security 10 interrogazioni", "9338109"),
                new InputItem("9338209", "9338209  - Check Security 20 interrogazioni", "9338209"),
                new InputItem("9110999", "9110999  - Consultazione dati in caso di disdetta", "9110999")
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }


        private void _AddActivity_Modulo(Workflow wf)
        {
            Activity a = wf.CreateActivity("upgradePEU");
            a.Title = "Vuoi attivare un upgrade?";
            a.TestoRiepilogo = "Upgrade da attivare";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                new InputItem("9318909", "9318909  - Upgrade incaricati Illimitati (per ogni gestione attiva)", "9318909"),
                new InputItem("9318059", "9318059  - Upgrade 5 incaricati cad. (per ogni gestione attiva)", "9318059"),
                new InputItem("9318209", "9318209  - Upgrade 20 incaricati cad. (per ogni gestione attiva)", "9318209"),
                new InputItem("9318509", "9318509  - Upgrade 50 incaricati cad. (per ogni gestione attiva)", "9318509"),
                new InputItem("9318999", "9318999  - Versione per DPO e Consulenti Privacy", "9318999")
            }));


            //a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
            //       new InputItem("{'Key':'upgradePEU','Text':'9318909 - Upgrade incaricati Illimitati (per ogni gestione attiva)','DataType':'integer','MinValue':1,'MaxValue':5,'DefaultValue':1, 'Tag':'9318909'}"),
            //       new InputItem("{'Key':'upgradePEU','Text':'9318059 - Upgrade 5 incaricati cad. (per ogni gestione attiva)','DataType':'integer','MinValue':1,'MaxValue':5,'DefaultValue':1, 'Tag':'9318059'}"),
            //       new InputItem("{'Key':'upgradePEU','Text':'9318209 - Upgrade 5 incaricati cad. (per ogni gestione attiva)','DataType':'integer','MinValue':1,'MaxValue':5,'DefaultValue':1, 'Tag':'9318209'}"),
            //       new InputItem("{'Key':'upgradePEU','Text':'9318509 - Upgrade 50 incaricati cad. (per ogni gestione attiva)','DataType':'integer','MinValue':1,'MaxValue':5,'DefaultValue':1, 'Tag':'9318509'}"),
            //       new InputItem("{'Key':'upgradePEU','Text':'9318999 - Versione per DPO e Consulenti Privacy','DataType':'integer','MinValue':1,'MaxValue':5,'DefaultValue':1, 'Tag':'9318999'}"),
            //             }));

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