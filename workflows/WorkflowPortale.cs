using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowPortale : Workflow
    {
        private Action<StateContext> _DrawPage { get; set; }
        //private bool possiedeContratto { get; set; }
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

        public WorkflowPortale(string key, string title, Action<StateContext> drawPage, /*bool possiedeContratto, */int tipoLicenza) : base(key, title)
        {
            _DrawPage = drawPage;

            List<string> methods = ShowMethods(typeof(WorkflowPortale));

            //this.possiedeContratto = possiedeContratto;
            this.tipoLicenza = tipoLicenza;

            foreach (string s in methods)
            {
                MethodInfo m = this.GetType().GetMethod(s, BindingFlags.NonPublic | BindingFlags.Instance);
                m.Invoke(this, new object[] { this });
            }
        }

        private void _AddActivity_TipoLicenza(Workflow wf)  //A.Lucchi
        {
            Activity a = wf.CreateActivity("lic");
            a.Title = "Che tipo di licenza desideri attivare?";
            a.TestoRiepilogo = "Tipo di licenza da attivare:";
            //a.Description = "Breve descrizione...";
            //a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
            //    //new InputItem("demo", "Demo"),
            //    //new InputItem("stan", "Standard"),
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

            //Branch b1 = a.CreateBranchTo("sogg");
            //b1.Condition.IfOutputContainsItem("demo");

            //Branch b2 = a.CreateBranchTo("sogg");
            //b2.Condition.IfOutputContainsItem("stan");

            //Branch b1 = a.CreateBranchTo("sogg");  //A.Lucchi(27/04/2021)
            Branch b1 = null;
            if (tipoLicenza == 0 || tipoLicenza > 2) //Se non ha nè PDS.AZI nè PDS.COMM o uno dei due articoli in delega
            {
                b1 = a.CreateBranchTo("sogg");
            }
            else
            {
                if (tipoLicenza == 1)
                {
                    b1 = a.CreateBranchTo("tipoModuloCOMM");
                }
                else
                {
                    b1 = a.CreateBranchTo("tipoModuloAZI");
                }
            }
        }

        //private void _AddActivity_TipoContratto(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("tipoContratto");
        //    a.Title = "Che tipo di contratto desideri attivare?";
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //        new InputItem("nuovo", "Nuovo contratto"),
        //        new InputItem("upgrade", "Upgrade di un contratto esistente"),
        //    }));
        //    a.DrawPage = _DrawPage;

        //    //Branch b1 = a.CreateBranchTo("lic");
        //    //b1.Condition.IfOutputContainsItem("nuovo");

        //    //Branch b2 = a.CreateBranchTo("upgrade");
        //    //b2.Condition.IfOutputContainsItem("upgrade");

        //    Branch b1 = a.CreateBranchTo("sogg");
        //}

        private void _AddActivity_TipoModuloCOMM(Workflow wf)
        {
            Activity a = wf.CreateActivity("tipoModuloCOMM");
            a.Title = "Quale tipo di modulo vuoi attivare?";
            a.TestoRiepilogo = "Tipo di modulo da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("moduloB2B", "Modulo B2B"),
                new InputItem("upgrade", "Upgrade"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("b2b_prof");
            b1.Condition.IfOutputContainsItem("moduloB2B");

            Branch b2 = a.CreateBranchTo("upgrade_prof");
            b2.Condition.IfOutputContainsItem("upgrade");
        }

        private void _AddActivity_TipoAzienda(Workflow wf)
        {
            Activity a = wf.CreateActivity("tipo_azi");
            a.Title = "Quale tipo di azienda vuoi attivare?";
            a.TestoRiepilogo = "Tipo di azienda da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("auto", "Azienda singola"),
                new InputItem("capo", "Azienda capogruppo"),
            }));
            a.DrawPage = _DrawPage;
            
            Branch b1 = a.CreateBranchTo("B2BAziAutoArticoli");
            b1.Condition.IfOutputContainsItem("auto");

            Branch b2 = a.CreateBranchTo("B2BAziCapoArticoli");
            b2.Condition.IfOutputContainsItem("capo");

        }

        private void _AddActivity_TipoModuloAZI(Workflow wf)
        {
            Activity a = wf.CreateActivity("tipoModuloAZI");
            a.Title = "Quale tipo di modulo vuoi attivare?";
            a.TestoRiepilogo = "Tipo di modulo da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("moduloB2B", "Modulo B2B"),
                new InputItem("upgrade", "Upgrade"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("tipo_azi");
            b1.Condition.IfOutputContainsItem("moduloB2B");
            
            Branch b2 = a.CreateBranchTo("upgrade_azi");
            b2.Condition.IfOutputContainsItem("upgrade");

        }


        private void _AddActivity_Soggetto(Workflow wf)
        {
            Activity a = wf.CreateActivity("sogg");
            a.Title = "Quale tipo di soggetto vuoi abilitare?";
            a.TestoRiepilogo = "Tipo di soggetto:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("prof", "Professionista", "9100104;PDS.COMM"),
                new InputItem("azi", "Azienda", "PDS.AZI"),
            }));
            a.DrawPage = _DrawPage;

            //Branch b1 = a.CreateBranchTo("serv_prof");
            Branch b1 = a.CreateBranchTo("tipoModuloCOMM");  //A.Lucchi(27/04/2021)
            b1.Condition.IfOutputContainsItem("prof");

            Branch b2 = a.CreateBranchTo("tipoModuloAZI");
            b2.Condition.IfOutputContainsItem("azi");
        }

        //private void _AddActivity_ServiziProfessionista(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("serv_prof");
        //    a.Title = "Quali servizi per il professionista vuoi attivare?";
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
        //        new InputItem("b2b", "B2B"),
        //        new InputItem("smo", "Spese Mediche Online"),
        //        new InputItem("pa", "Fatturazione elettronica PA e gestione ordini PA tramite NSO"),
        //        new InputItem("cs", "Conservazione Sostitutiva"),
        //        new InputItem("precom", "Adempimenti Precompilati"),
        //    }));
        //    a.DrawPage = _DrawPage;

        //    Branch b1 = a.CreateBranchTo("b2b_prof");
        //    b1.Condition.IfOutputContainsItem("b2b");

        //    //Branch b2 = a.CreateBranchToSummary();
        //    Branch b2 = a.CreateBranchTo("uploadFile");
        //}

        private void _AddActivity_B2BProfessionista(Workflow wf)
        {
            Activity a = wf.CreateActivity("b2b_prof");
            a.Title = "Quali moduli B2B vuoi attivare?";
            a.TestoRiepilogo = "Moduli del B2B da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("9900018", "9900018 - Per clienti Espando Studio Light (1 PDL) - Max 7.000 ft.", "9900018"),
                new InputItem("9900038", "9900038 - B2B per Studi ( 1 - 3   PDL) - max 12.000 ft.", "9900038"),
                new InputItem("9900058", "9900058 - B2B per Studi ( 4 - 5   PDL) - max 22.000 ft.", "9900058"),
                new InputItem("9900108", "9900108 - B2B per Studi ( 6 - 10  PDL) - max 32.000 ft.", "9900108"),
                new InputItem("9900158", "9900158 - B2B per Studi ( 11 - 15 PDL) - max 42.000 ft.", "9900158"),
                new InputItem("9900208", "9900208 - B2B per Studi ( 16 - 20 PDL) - max 52.000 ft.", "9900208"),
               // new InputItem("9900198", "9900198 - B2B per Studi (oltre 20 PDL)", "9900198"),
            }));
            a.DrawPage = _DrawPage;

            //Branch b1 = a.CreateBranchToSummary();
            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        //private void _AddActivity_ServiziAzienda(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("serv_azi");
        //    a.Title = "Quali servizi per l'azienda vuoi attivare?";
        //    a.TestoRiepilogo = "Servizi per l'azienda da attivare:";
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
        //        new InputItem("b2b", "B2B"),
        //        new InputItem("smo", "Spese Mediche Online"),
        //        new InputItem("pa", "Fatturazione elettronica PA e gestione ordini PA tramite NSO"),
        //        new InputItem("cs", "Conservazione Sostitutiva"),
        //        new InputItem("precom", "Adempimenti Precompilati"),
        //    }));
        //    a.DrawPage = _DrawPage;

        //    Branch b1 = a.CreateBranchTo("b2b_azi_auto");
        //    b1.Condition.IfOutputOfActivityContainsItem("tipo_azi", "auto").And.IfOutputContainsItem("b2b");

        //    Branch b2 = a.CreateBranchTo("b2b_azi_capo");
        //    b2.Condition.IfOutputOfActivityContainsItem("tipo_azi", "capo").And.IfOutputContainsItem("b2b");

        //    //Branch b3 = a.CreateBranchToSummary();
        //    Branch b3 = a.CreateBranchTo("uploadFile");
        //}

        private void _AddActivity_B2BAziAutoArticoli(Workflow wf)
        {
            Activity a = wf.CreateActivity("B2BAziAutoArticoli");
            a.Title = "Azienda autonoma - quale modulo vuoi attivare?";
            a.TestoRiepilogo = "Modulo da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("pds", "Portale dei servizi B2B"),
                new InputItem("pdsFol", "Portale dei Servizi B2B e Fatture Online","9100204")
            }));
            a.DrawPage = _DrawPage;

            //Branch b1 = a.CreateBranchToSummary();
            Branch b1 = a.CreateBranchTo("b2b_azi_auto");
            b1.Condition.IfOutputContainsItem("pds");

            Branch b2 = a.CreateBranchTo("b2b_azi_fol_auto");
            b2.Condition.IfOutputContainsItem("pdsFol");
        }

        private void _AddActivity_B2BAziCapoArticoli(Workflow wf)
        {
            Activity a = wf.CreateActivity("B2BAziCapoArticoli");
            a.Title = "Azienda capogruppo - quale modulo vuoi attivare?";
            a.TestoRiepilogo = "Modulo da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("pds", "Portale dei servizi B2B"),
                new InputItem("pdsFol", "Portale dei Servizi B2B e Fatture Online","9100204")
            }));
            a.DrawPage = _DrawPage;

            //Branch b1 = a.CreateBranchToSummary();
            Branch b1 = a.CreateBranchTo("b2b_azi_capo");
            b1.Condition.IfOutputContainsItem("pds");

            Branch b2 = a.CreateBranchTo("b2b_azi_fol_capo");
            b2.Condition.IfOutputContainsItem("pdsFol");

        }

        private void _AddActivity_B2BAziendaAutonoma(Workflow wf)
        {
            Activity a = wf.CreateActivity("b2b_azi_auto");
            a.Title = "Azienda autonoma - quali moduli B2B vuoi attivare?";
            a.TestoRiepilogo = "Moduli B2B da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("9920068", "9920068 - Portale dei Servizi B2B per Azienda Singola 1.000 ft.", "9920068"),
                new InputItem("9920128", "9920128 - Portale dei Servizi B2B per Azienda Singola 3.000 ft.", "9920128"),
                new InputItem("9920248", "9920248 - Portale dei Servizi B2B per Azienda Singola 5.000 ft.", "9920248"),
                new InputItem("9920428", "9920428 - Portale dei Servizi B2B per Azienda Singola 7.500 ft.", "9920428"),
                new InputItem("9920568", "9920568 - Portale dei Servizi B2B per Azienda Singola 10.000 ft.", "9920568"),
                new InputItem("9920718", "9920718 - Portale dei Servizi B2B per Azienda Singola 12.500 ft.", "9920718"),
                new InputItem("9920868", "9920868 - Portale dei Servizi B2B per Azienda Singola 15.000 ft.", "9920868"),
            }));
            a.DrawPage = _DrawPage;

            //Branch b1 = a.CreateBranchToSummary();
            Branch b1 = a.CreateBranchTo("b2b_fol_auto_azi");
        }

        private void _AddActivity_B2BFOLAziendaAutonoma(Workflow wf)
        {
            Activity a = wf.CreateActivity("b2b_azi_fol_auto");
            a.Title = "Azienda autonoma - quali moduli B2B vuoi attivare?";
            a.TestoRiepilogo = "Moduli B2B da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("9200223", "9200223 - Fatture Online e Portale Fattura per l’Azienda con 400 fatture tramitate", "9200223"),
                new InputItem("9200243", "9200243 - Fatture Online e Portale Fattura per l’Azienda con 600 fatture tramitate", "9200243"),
                new InputItem("9201203", "9201203 - Fatture Online e Portale Fattura per l’Azienda con 1.200 fatture tramitate", "9201203"),
                new InputItem("9202203", "9202203 - Fatture Online e Portale Fattura per l’Azienda con 2.200 fatture tramitate", "9202203")
            }));
            a.DrawPage = _DrawPage;

            //Branch b1 = a.CreateBranchToSummary();
            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_B2BFOLAziendaCapo(Workflow wf)
        {
            Activity a = wf.CreateActivity("b2b_azi_fol_capo");
            a.Title = "Quali moduli B2B vuoi attivare?";
            a.TestoRiepilogo = "Moduli B2B da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                  new InputItem("9200223", "9200223 - Fatture Online e Portale Fattura per l’Azienda con 400 fatture tramitate", "9200223"),
                new InputItem("9200243", "9200243 - Fatture Online e Portale Fattura per l’Azienda con 600 fatture tramitate", "9200243"),
                new InputItem("9201203", "9201203 - Fatture Online e Portale Fattura per l’Azienda con 1.200 fatture tramitate", "9201203"),
                new InputItem("9202203", "9202203 - Fatture Online e Portale Fattura per l’Azienda con 2.200 fatture tramitate", "9202203")
            }));
            a.DrawPage = _DrawPage;

            //Branch b1 = a.CreateBranchToSummary();
            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_B2BAziendaCapogruppo(Workflow wf)
        {
            Activity a = wf.CreateActivity("b2b_azi_capo");
            a.Title = "Quali moduli B2B vuoi attivare?";
            a.TestoRiepilogo = "Moduli B2B da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("9930128", "9930128 - Portale dei Servizi B2B per Azienda Capogruppo 3.000 ft.", "9930128"),
                new InputItem("9930248", "9930248 - Portale dei Servizi B2B per Azienda Capogruppo 5.000 ft.", "9930248"),
                new InputItem("9930428", "9930428 - Portale dei Servizi B2B per Azienda Capogruppo 7.500 ft.", "9930428"),
                new InputItem("9930568", "9930568 - Portale dei Servizi B2B per Azienda Capogruppo 10.000 ft.", "9930568"),
                new InputItem("9930718", "9930718 - Portale dei Servizi B2B per Azienda Capogruppo 12.500 ft.", "9930718"),
                new InputItem("9930868", "9930868 - Portale dei Servizi B2B per Azienda Capogruppo 15.000 ft.", "9930868"),
            }));
            a.DrawPage = _DrawPage;

            //Branch b1 = a.CreateBranchToSummary();
            Branch b1 = a.CreateBranchTo("b2b_fol_capo_azi");
        }

        private void _AddActivity_B2BFOLAZIAuto(Workflow wf)
        {
            Activity a = wf.CreateActivity("b2b_fol_auto_azi");
            a.Title = "Vuoi attivare anche Fatture Online?";
            a.TestoRiepilogo = "Moduli B2B da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("9200203", "9200203 - Portale Fattura per l’Azienda", "9200203")
            }));
            a.DrawPage = _DrawPage;
            a.AllowNoChoice = true;

            //Branch b1 = a.CreateBranchToSummary();
            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_B2BFOLAZICapo(Workflow wf)
        {
            Activity a = wf.CreateActivity("b2b_fol_capo_azi");
            a.Title = "Vuoi attivare anche Fatture Online?";
            a.TestoRiepilogo = "Moduli B2B da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("9200203", "9200203 - Portale Fattura per l’Azienda", "9200203")
            }));
            a.DrawPage = _DrawPage;
            a.AllowNoChoice = true;

            //Branch b1 = a.CreateBranchToSummary();
            Branch b1 = a.CreateBranchTo("uploadFile");
        }


        private void _AddActivity_B2BFOL(Workflow wf)
        {
            Activity a = wf.CreateActivity("b2b_fol");
            a.Title = "Vuoi attivare anche Fatture Online?";
            a.TestoRiepilogo = "Moduli B2B da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("9200223", "9200223 - Fatture Online e Portale Fattura per l’Azienda con 400 fatture tramitate (attive e passive)", "9200223"),
                new InputItem("9200243", "9200243 - Fatture Online e Portale Fattura per l’Azienda con 600 fatture tramitate (attive e passive)", "9200243"),
                new InputItem("9201203", "9201203 - Fatture Online e Portale Fattura per l’Azienda con 1.200 fatture tramitate (attive e passive)", "9201203"),
                new InputItem("9202203", "9202203 - Fatture Online e Portale Fattura per l’Azienda con 2.200 fatture tramitate (attive e passive)", "9202203")
            }));
            a.DrawPage = _DrawPage;

            //Branch b1 = a.CreateBranchToSummary();
            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        //private void _AddActivity_UpgradeSoggetto(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("upgrade");
        //    a.Title = "Quale tipo di soggetto vuoi abilitare?";
        //    a.TestoRiepilogo = "Tipo di soggetto da abilitare:";
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //        new InputItem("prof", "Professionista"),
        //        new InputItem("azi", "Azienda"),
        //    }));
        //    a.DrawPage = _DrawPage;

        //    Branch b1 = a.CreateBranchTo("upgrade_prof");
        //    b1.Condition.IfOutputContainsItem("prof");

        //    Branch b2 = a.CreateBranchTo("upgrade_azi");
        //    b2.Condition.IfOutputContainsItem("azi");
        //}

        private void _AddActivity_UpgradeProfessionista(Workflow wf)
        {
            Activity a = wf.CreateActivity("upgrade_prof");
            a.Title = "Quali moduli vuoi attivare? <span style='font-size:20px'>1 di 2</span>";
            a.TestoRiepilogo = "Moduli da abilitare:";
            //a.Description = "Breve descrizione...";
            //a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
            //    new InputItem("9910258", "9910258 - Ulteriore blocco da 2.500  ft. per studio", "9910258"),
            //    new InputItem("9910508", "9910508 - Ulteriore blocco da 5.000  ft. per studio", "9910508"),
            //    new InputItem("9911008", "9911008 - Ulteriore blocco da 10.000 ft. per studio", "9911008"),
            //    new InputItem("9913008", "9913008 - Ulteriore blocco da 30.000 ft. per studio", "9913008"),
            //    new InputItem("9915008", "9915008 - Ulteriore blocco da 50.000 ft. per studio", "9915008"),
            //}));

            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
                new InputItem("{'Key':'upgradePDS','Text':'9910258 - Ulteriore blocco da 2.500  ft. per studio','DataType':'integer','MinValue':1,'MaxValue':5,'DefaultValue':1, 'Tag':'9910258'}"),
                new InputItem("{'Key':'upgradePDS','Text':'9910508 - Ulteriore blocco da 5.000  ft. per studio','DataType':'integer','MinValue':1,'MaxValue':5,'DefaultValue':1, 'Tag':'9910508'}"),
                new InputItem("{'Key':'upgradePDS','Text':'9911008 - Ulteriore blocco da 10.000 ft. per studio','DataType':'integer','MinValue':1,'MaxValue':5,'DefaultValue':1, 'Tag':'9911008'}"),
                new InputItem("{'Key':'upgradePDS','Text':'9913008 - Ulteriore blocco da 30.000 ft. per studio','DataType':'integer','MinValue':1,'MaxValue':5,'DefaultValue':1, 'Tag':'9913008'}"),
                new InputItem("{'Key':'upgradePDS','Text':'9915008 - Ulteriore blocco da 50.000 ft. per studio','DataType':'integer','MinValue':1,'MaxValue':5,'DefaultValue':1, 'Tag':'9915008'}"),
            }));

            a.AllowNoChoice = true;
            a.DrawPage = _DrawPage;

            //Branch b1 = a.CreateBranchToSummary();
            Branch b1 = a.CreateBranchTo("upgrade_profMantenimentoInLinea");
        }

        private void _AddActivity_UpgradeProfessionistaMantenimentoFattureInLinea(Workflow wf)
        {
            Activity a = wf.CreateActivity("upgrade_profMantenimentoInLinea");
            a.Title = "Vuoi attivare il seguente modulo? <span style='font-size:20px'>2 di 2</span>";
            a.TestoRiepilogo = "Modulo da attivare:";

            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("6518990", "6518990 - Mantenimento in linea fatture sul portale per STUDIO", "6518990")
            }));

            a.DrawPage = _DrawPage;
            a.AllowNoChoice = true;

            //Branch b1 = a.CreateBranchToSummary();
            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_UpgradeAzienda(Workflow wf)
        {
            Activity a = wf.CreateActivity("upgrade_azi");
            a.Title = "Quali moduli vuoi attivare?";
            a.TestoRiepilogo = "Moduli da attivare:";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("b2b", "Ulteriori blocchi di fatture per Portale dei Servizi b2B"),
                new InputItem("b2bFOL", "Ulteriori blocchi di fatture per Portale dei servizi e Fatture Online"),
                new InputItem("b2bFOLAzi", "Upgrade Fatture Online"),
                new InputItem("6518991", "6518991 - Mantenimento in linea fatture sul portale per AZIENDA Fatture Online", "6518991"),
                new InputItem("6518992","6518992 - Mantenimento in linea fatture sul portale per AZIENDA con Accresco Impresa /  Italworking Revolution","6518992")
            }));

            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("upgrade_aziB2B");
            b1.Condition.IfOutputContainsItem("b2b");

            Branch b2 = a.CreateBranchTo("upgrade_aziB2BFol");
            b2.Condition.IfOutputContainsItem("b2bFOL");

            Branch b3 = a.CreateBranchTo("upgrade_aziB2BFolAzi");
            b3.Condition.IfOutputContainsItem("b2bFOLAzi");

            Branch b4 = a.CreateBranchTo("uploadFile");
        }


        //private void _AddActivity_UpgradeAziendaMantenimentoFattureInLinea(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("upgrade_aziMantenimentoInLinea");
        //    a.Title = "Vuoi attivare il seguente modulo?";
        //    a.TestoRiepilogo = "Modulo da attivare:";

        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //        new InputItem("6518991", "6518991 - Mantenimento in linea fatture sul portale per AZIENDA", "6518991", true)
        //    }));

        //    a.DrawPage = _DrawPage;

        //    //Branch b1 = a.CreateBranchToSummary();
        //    Branch b1 = a.CreateBranchTo("uploadFile");
        //}


        private void _AddActivity_UpgradeAziendaB2B(Workflow wf)
        {
            Activity a = wf.CreateActivity("upgrade_aziB2B");
            a.Title = "Quali moduli vuoi attivare?";
            a.TestoRiepilogo = "Moduli da attivare:";

            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
                new InputItem("{'Key':'upgradePDS','Text':'9940018 - Ulteriore blocco da 1.800  ft. per azienda','DataType':'integer','MinValue':1,'MaxValue':5,'DefaultValue':1, 'Tag':'9940018'}"),
                new InputItem("{'Key':'upgradePDS','Text':'9940258 - Ulteriore blocco da 3.000  ft. per azienda','DataType':'integer','MinValue':1,'MaxValue':5,'DefaultValue':1, 'Tag':'9940258'}"),
                new InputItem("{'Key':'upgradePDS','Text':'9940608 - Ulteriore blocco da 8.000  ft. per azienda','DataType':'integer','MinValue':1,'MaxValue':5,'DefaultValue':1, 'Tag':'9940608'}"),
                new InputItem("{'Key':'upgradePDS','Text':'9940858 - Ulteriore blocco da 15.000 ft. per azienda','DataType':'integer','MinValue':1,'MaxValue':5,'DefaultValue':1, 'Tag':'9940858'}"),
                new InputItem("{'Key':'upgradePDS','Text':'9942008 - Ulteriore blocco da 35.000 ft. per azienda','DataType':'integer','MinValue':1,'MaxValue':5,'DefaultValue':1, 'Tag':'9942008'}"),
            }));

            a.DrawPage = _DrawPage;

            //Branch b1 = a.CreateBranchToSummary();
            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_UpgradeAziendaB2BFOL(Workflow wf)
        {
            Activity a = wf.CreateActivity("upgrade_aziB2BFol");
            a.Title = "Quali moduli vuoi attivare?";
            a.TestoRiepilogo = "Moduli da attivare:";

            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
                new InputItem("{'Key':'upgradePDS','Text':'9210208 - Upgrade B2B 400 fatture tramitate','DataType':'integer','MinValue':1,'MaxValue':5,'DefaultValue':1, 'Tag':'9210208'}"),
                new InputItem("{'Key':'upgradePDS','Text':'9210408 - Upgrade B2B 600 fatture tramitate','DataType':'integer','MinValue':1,'MaxValue':5,'DefaultValue':1, 'Tag':'9210408'}"),
                new InputItem("{'Key':'upgradePDS','Text':'9211008 - Upgrade B2B 1.200 fatture tramitate','DataType':'integer','MinValue':1,'MaxValue':5,'DefaultValue':1, 'Tag':'9211008'}"),
                new InputItem("{'Key':'upgradePDS','Text':'9212008 - Upgrade B2B 2.200 fatture tramitate','DataType':'integer','MinValue':1,'MaxValue':5,'DefaultValue':1, 'Tag':'9212008'}")
            }));

            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_UpgradeAziendaB2BFol(Workflow wf)
        {
            Activity a = wf.CreateActivity("upgrade_aziB2BFolAzi");
            a.Title = "Quali moduli vuoi attivare?";
            a.TestoRiepilogo = "Moduli da attivare:";

            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
                new InputItem("{'Key':'upgradePDS','Text':'9940058 - 500 fatture','DataType':'integer','MinValue':1,'MaxValue':5,'DefaultValue':1, 'Tag':'9940058'}"),
                new InputItem("{'Key':'upgradePDS','Text':'9940108 - 1.000 fatture','DataType':'integer','MinValue':1,'MaxValue':5,'DefaultValue':1, 'Tag':'9940108'}")
            }));

            a.DrawPage = _DrawPage;

            //Branch b1 = a.CreateBranchToSummary();
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
            a.Title = "La procedura di attivazione si è conclusa";
            a.DrawPage = _DrawPage;
        }
    }
}
