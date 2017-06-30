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

        public async Task<IPrinters> Printers(object input)
        {
            IPrinters thePrinters;

            try
            {
                thePrinters = await Task.Run(() => new Printers());
            }
            catch (Exception ex)
            {
                throw ex;
            }
     
            return thePrinters;
        }

        /*
		 *
		 *
		 *
		 *
		 */
        public async Task<bool> Print(object args)
        {
            IDictionary<string, object> parameters = (IDictionary<string, object>)args;

            //
            // Make sure the required parts exist
            if (!parameters.ContainsKey("printer"))
            {
                Console.WriteLine("NodeDymoLibrary No `printer` parameter");
                throw new System.ArgumentException("'printer' parameter must be defined", "original");
            } else if (!parameters.ContainsKey("labels"))
            {
                Console.WriteLine("NodeDymoLibrary No `labels` parameter");
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
                Console.WriteLine("NodeDymoLibrary Adding Print Job Title: " + (string)parameters["jobTitle"]);
                printParams.JobTitle = (string)parameters["jobTitle"];
            }
            if( parameters.ContainsKey("copies") )
            {
                Console.WriteLine("NodeDymoLibrary Adding Print Copies: " + (string)parameters["copies"]);
                printParams.Copies = (int)parameters["copies"];
            }
            // Set some settings for this printing
            //

            IPrintJob printJob = printer.CreatePrintJob( printParams );
            Console.WriteLine("NodeDymoLibrary Print Job Created");

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
                if( !thisLabel.ContainsKey("filename") )
                {
                    Console.WriteLine("Dymo.cs No `labels`[x].`filename` parameter");
                    throw new System.ArgumentException("'labels'.'filename' parameter must be defined for each label", "original");
                }
                if( !File.Exists( (string)thisLabel["filename"] ))
                {
                    Console.WriteLine("Dymo.cs Unable to find label filename: " + (string)thisLabel["filename"]);
                    throw new System.ArgumentException("'labels'.'filename' parameter must point to an existing file", "original");
                }

                Console.WriteLine("NodeDymoLibrary Adding label: " + (string)thisLabel["filename"] );
                label[i] = Label.Open( (string)thisLabel["filename"] );
                if( thisLabel.ContainsKey("fields") )
                {
                    Console.WriteLine("NodeDymoLibrary Setting Field Values");
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
                        catch (Exception ex)
                        {
                            throw ex;
                        }

                    }
                }

                if( thisLabel.ContainsKey("images") )
                {
                    Console.WriteLine("NodeDymoLibrary Setting Image Values");
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
                            throw ex;
                        }
                    }
                }


                Console.WriteLine("NodeDymoLibrary Add Label to print job");
                printJob.AddLabel(label[i]);
            }


            Console.WriteLine("NodeDymoLibrary Lets Print the Label/s");
            try
            {
                await Task.Run(() => printJob.Print());
            }
            catch (Exception ex)
            {
                throw ex;
            }

            Console.WriteLine("NodeDymoLibrary Label/s Printed");
            return true;
        }
    }
}
