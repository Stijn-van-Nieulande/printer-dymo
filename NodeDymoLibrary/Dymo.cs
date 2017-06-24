using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DYMO.Label.Framework;

namespace NodeDymoLib
{
    public class Dymo
    {

        public async Task<object> Printers(object input)
        {
            IPrinters printers = new Printers();
            return printers;
        }

        /*
		 *
		 *
		 *
		 *
		 */
        public async Task<object> Print(object args)
        {
            IDictionary<string, object> parameters = (IDictionary<string, object>)args;

            //
            // Make sure the required parts exist
            if (!parameters.ContainsKey("printer"))
            {
                Console.WriteLine("Dymo.cs No `printer` parameter");
                throw new System.ArgumentException("'printer' parameter must be defined", "original");
            } else if (!parameters.ContainsKey("labels"))
            {
                Console.WriteLine("Dymo.cs No `labels` parameter");
                throw new System.ArgumentException("'labels' parameter must be defined", "original");
            }

            //
            // Setup the printer and this job
            //ILabelWriterPrinter printer = Framework.GetLabelWriterPrinters()[(String)parameters["printer"]] as ILabelWriterPrinter;
            IPrinter printer = Framework.GetPrinters()[(String)parameters["printer"]] as IPrinter;

            //
            // Set some settings for this printing
            ILabelWriterPrintParams printParams = new LabelWriterPrintParams();
            printParams.PrintQuality = LabelWriterPrintQuality.BarcodeAndGraphics;
            printParams.JobTitle = "Dymo Labels";
            printParams.Copies = (int)1;
            if ( parameters.ContainsKey("jobTitle") )
            {
                Console.WriteLine("Dymo.cs Adding Print Job Title: " + (string)parameters["jobTitle"]);
                printParams.JobTitle = (string)parameters["jobTitle"];
            }
            if( parameters.ContainsKey("copies") )
            {
                Console.WriteLine("Dymo.cs Adding Print Copies: " + (string)parameters["copies"]);
                printParams.Copies = (int)parameters["copies"];
            }
            // Set some settings for this printing
            //

            IPrintJob printJob = printer.CreatePrintJob( printParams );

            //
            // Lets loop over these labels
            IDictionary<string, ILabel> label = new Dictionary<string, ILabel>();
            object[] suppliedLabels = (object[])parameters["labels"];
            foreach (IDictionary<string, object> thisLabel in suppliedLabels)
            {
                var i = label.Count.ToString();
                //var i = lkv.Key.ToUpper();
                //IDictionary<string, object> thisLabel = (IDictionary<string, object>) lkv.Value;

                //IDictionary<string, object> thisLabel = (IDictionary<string, object>)suppliedLabels[i];
                if(  !thisLabel.ContainsKey("filename"))
                {
                    Console.WriteLine("Dymo.cs No `labels`[x].`filename` parameter");
                    throw new System.ArgumentException("'labels'.'filename' parameter must be defined for each label", "original");
                }

                label[i] = Label.Open( (string)thisLabel["filename"] );
                if(thisLabel.ContainsKey("fields") )
                {
                    IDictionary<string, object> fields = (IDictionary<string, object>)thisLabel["fields"];
                    foreach (var kv in fields)
                    {
                        try
                        {
                            var k = kv.Key.ToUpper();
                            var obj = label[i].GetObjectByName(k);
                            if (obj != null)
                            {
                                var v = kv.Value.ToString();
                                label[i].SetObjectText(k, v);
                            }
                        }
                        catch (Exception ex) { }

                    }
                }

                if (null != thisLabel["images"])
                {
                    IDictionary<string, object> images = (IDictionary<string, object>)thisLabel["images"];
                    foreach (var kv in images)
                    {
                        try
                        {
                            var k = kv.Key.ToUpper();
                            var obj = label[i].GetObjectByName(k);
                            if (obj != null)
                            {
                                var v = kv.Value;
                                label[i].SetImagePngData(k, new MemoryStream((byte[])v));
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }

                printJob.AddLabel(label[i]);
            }

            
            await Task.Run(() => printJob.Print() );
            return args;
        }
    }
}
