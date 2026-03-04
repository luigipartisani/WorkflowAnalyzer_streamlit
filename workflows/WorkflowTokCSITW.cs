using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowTokCSITW : Workflow
    {
        private Action<StateContext> _DrawPage { get; set; }

        private List<string> GetActivities(Type type)
        {
            List<string> activities = new List<string>();

            foreach (var method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (method.Name.StartsWith("_AddActivity_")) activities.Add(method.Name);
            }

            return activities;
        }

        public WorkflowTokCSITW(string key, string title, Action<StateContext> drawPage) : base(key, title)
        {
            _DrawPage = drawPage;

            List<string> activities = GetActivities(typeof(WorkflowTokCSITW));

            foreach (string a in activities)
            {
                MethodInfo m = this.GetType().GetMethod(a, BindingFlags.NonPublic | BindingFlags.Instance);
                m.Invoke(this, new object[] { this });
            }
        }

        private void _AddActivity_TipoLicenza(Workflow wf)
        {
            Activity a = wf.CreateActivity("lic");
            a.Title = "Che tipo di licenza desideri attivare?";
            a.TestoRiepilogo = "Tipo di licenza da attivare:";

            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                 new InputItem("Demo","Demo"),
                  new InputItem("Standard", "Standard"),
            }));

            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("tipoModuloCS");
            b1.Condition.IfOutputContainsItem("Standard");

            Branch b2 = a.CreateBranchTo("tipoModuloCSDEMO");
            b2.Condition.IfOutputContainsItem("Demo");
        }

        private void _AddActivity_TipoModuloCS(Workflow wf)
        {
            Activity a = wf.CreateActivity("tipoModuloCS");
            a.Title = "Che tipo di modulo desideri attivare?";
            a.TestoRiepilogo = "Tipo di modulo:";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("modulo", "Modulo base"),
                new InputItem("bundle", "Bundle"),
                new InputItem("upgrade", "Upgrade"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("tokCSModuli");
            b1.Condition.IfOutputContainsItem("modulo");

            Branch b2 = a.CreateBranchTo("tokCSBundle");
            b2.Condition.IfOutputContainsItem("bundle");

            Branch b3 = a.CreateBranchTo("tokCSUpgrade");
            b3.Condition.IfOutputContainsItem("upgrade");
        }

        private void _AddActivity_TipoModuloCSDEMO(Workflow wf)
        {
            Activity a = wf.CreateActivity("tipoModuloCSDEMO");
            a.Title = "Che tipo di modulo desideri attivare?";
            a.TestoRiepilogo = "Tipo di modulo:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("modulo", "Modulo base"),
                new InputItem("bundle", "Bundle")
                //new InputItem("bundleNoCS", "Bundle Fatture elettroniche B2B"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("tokCSModuliDEMO");
            b1.Condition.IfOutputContainsItem("modulo");

            Branch b2 = a.CreateBranchTo("tokCSBundle");
            b2.Condition.IfOutputContainsItem("bundle");
        }

        private void _AddActivity_TipoCSModuli(Workflow wf)
        {
            Activity a = wf.CreateActivity("tokCSModuli");
            a.Title = "Che moduli desideri attivare?";
            a.TestoRiepilogo = "Moduli da attivare:";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                new InputItem("6100011","6100011 Entratel Suite CS - Gestione conservazione cliente","6100011"),
                new InputItem("6100021","6100021 Entratel Suite Cs - Conservazione Sostitutiva Gestione Lul","6100021"),
                new InputItem("6100041","6100041 Entratel Suite Cs - Importazione Documenti Contabili Fatture Attive E Passive","6100041")
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_TipoCSModuliDEMO(Workflow wf)
        {
            Activity a = wf.CreateActivity("tokCSModuliDEMO");
            a.Title = "Che moduli desideri attivare?";
            a.TestoRiepilogo = "Moduli da attivare:";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                //new InputItem("6008111","6008111 - Upgrade per conservare fatture elettroniche","6008111"),
                new InputItem("6000051","6000051 - Entratel Suite Cs - Conservazione Sostitutiva Modulo Base 500 Anagrafiche","6000051")
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        //private void _AddActivity_TipoCSModuliFatture(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("fattureB2BCS");
        //    a.Title = "Che moduli desideri attivare?";
        //    a.TestoRiepilogo = "Moduli da attivare:";
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //        new InputItem("6108073","6108073 - Gestione fatture elettroniche B2B attive e passive 7.000","6108073"),
        //        new InputItem("6108253","6108253 - Gestione fatture elettroniche B2B attive e passive 25.000","6108253"),
        //        new InputItem("6108503","6108503 - Gestione fatture elettroniche B2B attive e passive 50.000","6108503"),
        //        new InputItem("6108993","6108993 - Gestione fatture elettroniche B2B attive e passive illimitate","6108993")
        //    }));
        //    a.DrawPage = _DrawPage;

        //    Branch b1 = a.CreateBranchTo("fattureB2BCS");
        //    b1.Condition.IfOutputContainsItem("6108031");

        //    Branch b2 = a.CreateBranchTo("uploadFile");
        //}

        private void _AddActivity_TipoCSUpgrade(Workflow wf)
        {
            Activity a = wf.CreateActivity("tokCSUpgrade");
            a.Title = "Che articoli upgrade desideri attivare?";
            a.TestoRiepilogo = "Articoli upgrade da attivare:";

            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
                new InputItem("{'Key':'upgradeTOKCS','Text':'6000151 Upgrade Conservazione Sostitutiva - Ulteriori 500 Anagrafiche Soggetti','DataType':'integer','MinValue':1,'MaxValue':10,'DefaultValue':1, 'Tag':'6000151'}"),
                new InputItem("{'Key':'upgradeTOKCS','Text':'6000311 Upgrade Conservazione Sostitutiva - Ulteriori 100 Anagrafiche Clienti Dello Studio','DataType':'integer','MinValue':1,'MaxValue':10,'DefaultValue':1, 'Tag':'6000311'}"),
                new InputItem("{'Key':'upgradeTOKCS','Text':'6000211 Upgrade -  Responsabile Conservazione Aggiuntivo','DataType':'integer','MinValue':1,'MaxValue':10,'DefaultValue':1,'Tag':'6000211'}"),
                new InputItem("{'Key':'upgradeTOKCS','Text':'6000411  Upgrade - Conservazione Sostitutiva per Consulenti del Lavoro (ulteriori 100 anagrafiche)','DataType':'integer','MinValue':1,'MaxValue':10,'DefaultValue':1,'Tag':'6000411'}")
            }));

            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_tokCSBundle(Workflow wf)
        {
            Activity a = wf.CreateActivity("tokCSBundle");
            a.Title = "Che tipo di conservazione desideri attivare?";
            a.TestoRiepilogo = "Tipo di conservazione da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("6000011","6000011 Entratel Suite Cs - Bundle per Consulenti Del Lavoro","6000011")
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
