using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowNSO : Workflow
    {
        //public bool possiedeContratto { get; set; }
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


        public WorkflowNSO(string key, string title, Action<StateContext> drawPage/*, bool possiedeContratto*/, int tipoLicenza) : base(key, title)
        {
            _DrawPage = drawPage;

            //this.possiedeContratto = possiedeContratto;
            this.tipoLicenza = tipoLicenza;

            List<string> methods = ShowMethods(typeof(WorkflowNSO));

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
            if (tipoLicenza == 0)
            {
                b1 = a.CreateBranchTo("tipoSogg");
            }
            else
            {
                b1 = a.CreateBranchTo("attivaNSO");
            }
        }

        private void _AddActivity_Soggetto(Workflow wf)
        {
            Activity a = wf.CreateActivity("tipoSogg");
            a.Title = "Quale tipo di soggetto vuoi abilitare?";
            a.TestoRiepilogo = "Tipo di soggetto:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("prof", "Professionista","PDS.COMM"),
                new InputItem("azi", "Azienda","PDS.AZI"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("attivaNSO");
        }

        //private void _AddActivity_TipoModuli(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("tipoModuli");
        //    a.Title = "Quale tipo di moduli vuoi abilitare?";
        //    a.TestoRiepilogo = "Tipo di moduli:";
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //        //new InputItem("PA", "Fatturazione elettronica PA"),
        //        new InputItem("NSO", "NSO", "9100099"),
        //        //new InputItem("Upgrade", "Upgrade"),
        //        //new InputItem("CS", "Mantenimento CS"),
        //    }));
        //    a.DrawPage = _DrawPage;

        //    //Branch b1 = a.CreateBranchTo("numeroPA");
        //    //b1.Condition.IfOutputContainsItem("PA");

        //    Branch b2 = a.CreateBranchTo("numeroNSO");
        //    b2.Condition.IfOutputContainsItem("NSO");

        //    //Branch b3 = a.CreateBranchTo("numeroPAUpgrade");
        //    //b3.Condition.IfOutputContainsItem("Upgrade");

        //    //Branch b4 = a.CreateBranchTo("mantenimentoCS");
        //    //b4.Condition.IfOutputContainsItem("CS");
        //}

        //private void _AddActivity_TipoContratto(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("tipoContratto");
        //    a.Title = "Che tipo di contratto vuoi attivare?";
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //       new InputItem("nContr", "Nuovo contratto"),
        //       new InputItem("upgrade", "Upgrade di un contratto esistente"),
        //    }));
        //    a.DrawPage = _DrawPage;

        //    Branch b1 = a.CreateBranchTo("attivaNSO");
        //    b1.Condition.IfOutputContainsItem("nContr");

        //    Branch b2 = a.CreateBranchTo("numeroPAUpgrade");
        //    b2.Condition.IfOutputContainsItem("upgrade");
        //}

        private void _AddActivity_AttivaNSO(Workflow wf)
        {
            Activity a = wf.CreateActivity("attivaNSO");
            a.Title = "Desideri attivare Nso?";
            a.TestoRiepilogo = "Modulo da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("nso", "Attivazione NSO con inclusi 36 ordini PA","9100099", true)
            }));
            a.DrawPage = _DrawPage;
            a.AllowNoChoice = true;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        //private void _AddActivity_NumeroPA(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("numeroPA");
        //    a.Title = "Vuoi acquistare pacchetti di fatture/ordini PA?";
        //    a.TestoRiepilogo = "Pacchetti di fatture/ordini PA:";
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
        //        new InputItem("9112093", "9112093 - 12  fatture/ordini PA per anno", "9112093"),
        //        new InputItem("9124093", "9124093 - 24  fatture/ordini PA per anno", "9124093"),
        //        new InputItem("9136093", "9136093 - 36  fatture/ordini PA per anno", "9136093"),
        //        new InputItem("9110093", "9110093 - 100 fatture/ordini PA per anno", "9110093"),
        //        new InputItem("9115003", "9115003 - 500 fatture/ordini PA per anno", "9115003")
        //    }));
        //    a.DrawPage = _DrawPage;

        //    Branch b1 = a.CreateBranchTo("uploadFile");
        //}

        //private void _AddActivity_NumeroNSO(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("numeroNSO");
        //    a.Title = "Vuoi acquistare pacchetti di fatture/ordini NSO?";
        //    a.TestoRiepilogo = "Pacchetti di fatture/ordini NSO:";
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
        //        new InputItem("9112093", "9112093 - 12  fatture/ordini PA per anno", "9112093;9100099"),
        //        new InputItem("9124093", "9124093 - 24  fatture/ordini PA per anno", "9124093;9100099"),
        //        new InputItem("9136093", "9136093 - 36  fatture/ordini PA per anno", "9136093;9100099"),
        //        new InputItem("9110093", "9110093 - 100 fatture/ordini PA per anno", "9110093;9100099"),
        //        new InputItem("9115003", "9115003 - 500 fatture/ordini PA per anno", "9115003;9100099")
        //    }));
        //    a.AllowNoChoice = true;
        //    a.DrawPage = _DrawPage;

        //    Branch b1 = a.CreateBranchTo("uploadFile");
        //}

        //private void _AddActivity_MantenimentoCS(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("mantenimentoCS");
        //    a.Title = "Mantenere la conservazione?";
        //    a.TestoRiepilogo = "Mantenere la conservazione:";
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //        new InputItem("910","910 - Mantenimento in Conservazione Sostitutiva dei dati e loro consultazione in caso di disdetta del Servizio ","910"),
        //    }));
        //    a.DrawPage = _DrawPage;
        //    a.AllowNoChoice = true;

        //    Branch b1 = a.CreateBranchTo("uploadFile");
        //}

        //private void _AddActivity_ModuliNSO(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("moduliNSO");
        //    a.Title = "Modulo NSO";
        //    a.TestoRiepilogo = "Modulo NSO:";
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //         new InputItem("9112093", "9112093 - Fatturazione Elettronica PA - 12 fatture/anno", "9112093"),
        //        new InputItem("9124093", "9124093 - Fatturazione Elettronica PA - 24 fatture/anno", "9124093"),
        //        new InputItem("9136093", "9136093 - Fatturazione Elettronica PA - 36 fatture/anno", "9136093"),
        //        new InputItem("9110093", "9110093 - Fatturazione Elettronica PA - 100 fatture/anno", "9110093"),
        //        new InputItem("9115003", "9115003 - Fatturazione Elettronica PA - 500 fatture/anno", "9115003")
        //    }));
        //    a.DrawPage = _DrawPage;
        //    a.AllowNoChoice = true;

        //    Branch b1 = a.CreateBranchTo("uploadFile");
        //}

        //private void _AddActivity_NumeroPAUpgrade(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("numeroPAUpgrade");
        //    a.Title = "Vuoi effetturare l'upgrade del contratto?";
        //    a.TestoRiepilogo = "Effettuare l'upgrade del contratto:";
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //        new InputItem("9124094", "9124094 - 24 fatture/ordini PA per anno ", "9124094"),
        //        new InputItem("9136094", "9136094 - 36 fatture/ordini PA per anno ", "9136094"),
        //        new InputItem("9110094", "9110094 - 100 fatture/ordini PA per anno ", "9110094"),
        //        new InputItem("9150094", "9150094 - 500 fatture/ordini PA per anno ", "9150094"),
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