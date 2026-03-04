using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowFEA : Workflow
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

        public WorkflowFEA(string key, string title, Action<StateContext> drawPage) : base(key, title)
        {
            _DrawPage = drawPage;

            List<string> methods = ShowMethods(typeof(WorkflowFEA));

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

            Branch b1 = a.CreateBranchTo("firmaOTP");
        }

        private void _AddActivity_Versione(Workflow wf)
        {
            Activity a = wf.CreateActivity("firmaOTP");
            a.Title = "Firma remota: quante firme desideri attivare?";
            a.TestoRiepilogo = "Numero di firme da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                //new InputItem("SIG.OTP.500", "SIG.OTP.500 - SMARTSIGN - FIRMA OTP (500 FIRME)","SIG.OTP.500"  ),
                //new InputItem("SIG.OTP.1500", "SIG.OTP.1500 - SMARTSIGN - FIRMA OTP (1500 FIRME)" ,"SIG.OTP.1500"  ),
                //new InputItem("SIG.OTP.CONS", "SIG.OTP.CONS - SMARTSIGN - FIRMA OTP (CONSUNTIVO)" ,"SIG.OTP.CONS"  ),
                //new InputItem("SIG.OTP.300", "SIG.OTP.300 - SMARTSIGN - FIRMA OTP (300 FIRME)","SIG.OTP.300"  ),
                //new InputItem("SIG.OTP.CONS", "SIG.OTP.CONS - Attivazione servizio Firma remota con OTP" ,"SIG.OTP.CONS"  ),
                new InputItem("SIG.OTP.100",  "SIG.OTP.100 - Smartsign - Firma OTP (100 firme)","SIG.OTP.100"  ),
                new InputItem("SIG.OTP.300",  "SIG.OTP.300 - Smartsign - Firma OTP (300 firme)","SIG.OTP.300"  ),
                new InputItem("SIG.OTP.500",  "SIG.OTP.500 - Smartsign - Firma OTP (500 firme)","SIG.OTP.500"  ),
                new InputItem("SIG.OTP.1500", "SIG.OTP.1500 - Smartsign - Firma OTP (1500 firme)" ,"SIG.OTP.1500"  ),
                new InputItem("SIG.OTP.5000", "SIG.OTP.5000 - Smartsign - Firma OTP (5000 firme)","SIG.OTP.5000"  ),
            }));
            a.DrawPage = _DrawPage;

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