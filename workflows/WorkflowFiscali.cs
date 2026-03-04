using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowFiscali : Workflow
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

        public WorkflowFiscali(string key, string title, Action<StateContext> drawPage) : base(key, title)
        {
            _DrawPage = drawPage;

            List<string> methods = ShowMethods(typeof(WorkflowFiscali));

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
            //     new InputItem("{'Key':'tipoLicenza','Text':'Demo','DataType':'radioText', 'Tag':'tipoLicenza','Style':'hidden','Index':0}"),
            //      new InputItem("{'Key':'tipoLicenza','Text':'Standard','DataType':'radioText', 'Tag':'tipoLicenza', 'Style':'hidden','Index':1}"),
            //}));

            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                 new InputItem("Demo","Demo"),
                  new InputItem("Standard", "Standard"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("tipoModulo");
        }

        private void _AddActivity_TipoContratto(Workflow wf)
        {
            Activity a = wf.CreateActivity("tipoModulo");
            a.Title = "Che tipo modulo desideri attivare?";
            a.TestoRiepilogo = "Tipo di modulo da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
               new InputItem("singAd", "Adempimento singolo"),
               new InputItem("bundle", "Bundle")
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("singAd");
            b1.Condition.IfOutputOfActivityContainsItem("lic", "Standard").And.IfOutputContainsItem("singAd");

            Branch b2 = a.CreateBranchTo("bundle");
            b2.Condition.IfOutputContainsItem("bundle");

            Branch b3 = a.CreateBranchTo("singAdDemo");
            b3.Condition.IfOutputOfActivityContainsItem("lic", "Demo").And.IfOutputContainsItem("singAd");
        }

        private void _AddActivity_AttivaSingoliAdempimenti(Workflow wf)
        {
            Activity a = wf.CreateActivity("singAd");
            a.Title = "Che moduli desideri attivare?";
            a.TestoRiepilogo = "Moduli da attivare:";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                new InputItem("7808003", "7808003 - Bilancio CEE e Nota Integrativa", "7808003"),
                new InputItem("7818003", " 7818003 - Analisi di bilancio (Upgrade al Bilancio CEE)", "7818003"),
                new InputItem("7828003", "7828003 - Bilancio consolidato", "7828003"),
                new InputItem("1108003", "1108003 - Iva – Dichiarazione dei Sostituti d’imposta", "1108003"),
                new InputItem("7718003", "7718003 - 770 Dichiarazione dei Sostituti d'Imposta", "7718003"),
                new InputItem("7408003", "7408003 - Unico Persone Fisiche (solo redditi) + Irap ", "7408003"),
                new InputItem("7508003", "7508003 - Unico Società di Persone (solo redditi) + Irap", "7508003"),
                new InputItem("7608003", "7608003 - Unico società di Capitali (solo redditi) + Irap", "7608003"),
                new InputItem("9308003", "9308003 - Calcolo imposte Correnti – Anticipate – Differite", "9308003"),
                new InputItem("8208003", "8208003 - Imu – Imposta Municipale Unica", "8208003"),
                new InputItem("8308003", "8308003 - Tasi – Tassa sui Servizi indivisibili", "8308003"),
                new InputItem("8408003", "8408003 - F24-F23 e Comunicazioni IVA", "8408003"),
                new InputItem("7208003", "7208003 - CU – Certificazione Unica", "7208003"),
                new InputItem("7218003", "7218003 - Certificazioni (Upgrade alla CU - Certificazione Unica)", "7218003"),
                new InputItem("5008003", "5008003 - Comunicazione unica", "5008003"),
                new InputItem("2948001", "2948001 - Redditometro", "2948001"),
                new InputItem("7308013", "7308013 - 730 Professionisti", "7308013"),
                new InputItem("7309003", "7309003 - 730 CAF generico ", "7309003"),
                //new InputItem("2978001", "2978001 - Firma digitale Dike 6 PRO ", "2978001")
            }));
            a.DrawPage = _DrawPage;

            //Branch b1 = a.CreateBranchTo("upgradeCU");
            //b1.Condition.IfOutputContainsItem("7808003").And.IfOutputOfActivityContainsItem("lic","Standard");

            //Branch b2 = a.CreateBranchTo("upgradeCU");
            //b2.Condition.IfOutputContainsItem("7208003").And.IfOutputOfActivityContainsItem("lic", "Standard");

            Branch b3 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_AttivaSingoliAdempimentiDEMO(Workflow wf)
        {
            Activity a = wf.CreateActivity("singAdDemo");
            a.Title = "Che moduli desideri attivare?";
            a.TestoRiepilogo = "Moduli da attivare:";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                new InputItem("7808003", "7808003 - Bilancio CEE e Nota Integrativa", "7808003"),
                new InputItem("7818003", " 7818003 - Analisi di bilancio (Upgrade al Bilancio CEE)", "7818003"),
                new InputItem("7828003", "7828003 - Bilancio consolidato", "7828003"),
                new InputItem("1108003", "1108003 - Iva – Dichiarazione dei Sostituti d’imposta", "1108003"),
                new InputItem("7718003", "7718003 - 770 Dichiarazione dei Sostituti d'Imposta", "7718003"),
                new InputItem("7408003", "7408003 - Unico Persone Fisiche (solo redditi) + Irap ", "7408003"),
                new InputItem("7508003", "7508003 - Unico Società di Persone (solo redditi) + Irap", "7508003"),
                new InputItem("7608003", "7608003 - Unico società di Capitali (solo redditi) + Irap", "7608003"),
                new InputItem("9308003", "9308003 - Calcolo imposte Correnti – Anticipate – Differite", "9308003"),
                new InputItem("8208003", "8208003 - Imu – Imposta Municipale Unica", "8208003"),
                new InputItem("8408003", "8408003 - F24-F23 e Comunicazioni IVA", "8408003"),
                new InputItem("7208003", "7208003 - CU – Certificazione Unica", "7208003"),
                new InputItem("7218003", "7218003 - Certificazioni (Upgrade alla CU - Certificazione Unica)", "7218003"),
                new InputItem("5008003", "5008003 - Comunicazione unica", "5008003"),
                new InputItem("7308013", "7308013 - 730 Professionisti", "7308013"),
                new InputItem("7309003", "7309003 - 730 CAF generico ", "7309003"),
                //new InputItem("2978001", "2978001 - Firma digitale Dike 6 PRO ", "2978001")
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_AttivaBundle(Workflow wf)
        {
            Activity a = wf.CreateActivity("bundle");
            a.Title = "Che moduli desideri attivare?";
            a.TestoRiepilogo = "Moduli da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("9408004", "9408004 - UNICO Persone Fisiche - Completo", "9408004"),
                new InputItem("9508004", "9508004 - UNICO Società di Persone - Completo", "9508004"),
                new InputItem("9608004", "9608004 - UNICO Società di Capitali ed Enti - Completo", "9608004"),
                new InputItem("9708004", "9708004 - UNICO Persone Fisiche + Società di Persone - Completo", "9708004"),
                new InputItem("9808004", "9808004 - UNICO Persone Fisiche + Società di Capitali ed Enti - Completo", "9808004"),
                new InputItem("8518002", "8518002 - Tutto Adempimenti Fiscali Plus", "8518002"),
                new InputItem("9108104", "9108104 - TuttUnico Società di Capitali ed Enti", "9108104"),
                new InputItem("9118104", "9118104 - TuttUnico per Amministrazioni ed Enti Pubblici", "9118104"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        //private void _AddActivity_UpgradeCU(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("upgradeCU");
        //    a.Title = "Che moduli desideri attivare?";
        //    a.TestoRiepilogo = "Moduli da attivare:";
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
        //        //new InputItem("7818003", " 7818003 - Analisi di Bilancio", "7818003"),
        //        //new InputItem("7218003", "7218003 - Certificazione", "7218003")
        //    }));
        //    a.DrawPage = _DrawPage;
        //    a.AllowNoChoice = true;

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