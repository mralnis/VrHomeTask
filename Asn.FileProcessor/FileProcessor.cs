using Asn.FileProcessor.Data;
using ASN.Infrastructure.Data;
using ASN.Infrastructure.Data.Models;
using System.Text.RegularExpressions;

namespace Asn.FileProcessor
{
    public class FileProcessor(UnitOfWork unitOfWork)
    {
        private readonly UnitOfWork _unitOfWork = unitOfWork;

        public void Process(string filePath)
        {
            long fileLocationIndex = 0;

            bool isEndOfFile = false;
            while (!isEndOfFile)
            {
                var acknowledgementShippingNotification = GetOneBoxEntry(filePath, ref fileLocationIndex);

                if (acknowledgementShippingNotification != null)
                {
                    ProcessChunk(acknowledgementShippingNotification);
                }

                if (fileLocationIndex == -1 || acknowledgementShippingNotification == null)
                {
                    isEndOfFile = true;
                }
            }
        }

        private Box GetOneBoxEntry(string filePath, ref long index)
        {
            Box? box = null;

            using (FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                fs.Position = index;

                using (StreamReader reader = new(fs))
                {
                    box = CreateBoxFromHeader(reader, ref index);

                    if (box == null)
                    {
                        throw new Exception("Box header not found");
                    }

                    AddAllBoxContent(box, reader, ref index);

                    if (box.Contents.Count == 0)
                    {
                        throw new Exception("Box content not found");
                    }

                    fs.Close();
                    return box;
                }
            }
        }

        private void AddAllBoxContent(Box? box, StreamReader reader, ref long index)
        {
            bool nextBoxFound = false;
            while (!nextBoxFound)
            {
                var contentLine = reader.ReadLine();

                if (contentLine == string.Empty)
                {
                    AddEmptyLineLenght(ref index);
                    continue;
                }

                if (contentLine == null)
                {
                    index = -1;
                    return;
                }

                if (contentLine.Contains("HDR"))
                {
                    nextBoxFound = true;
                    continue;
                }
                else
                {
                    index = GetCurrnetStreamLocationIndex(index, contentLine);
                    string[] operands = Regex.Split(contentLine.Trim(), @"\s+");

                    if (operands.Length == 4)
                    {
                        //ToDO - Add more validation here to verify that the values are correct
                        box.Contents.Add(new()
                        {
                            PoNumber = operands[1],
                            Isbn = operands[2],
                            Quantity = int.Parse(operands[3])
                        });
                    }
                    else
                    {
                        throw new Exception($"Invalid Content Line: {contentLine}");
                    }
                }
            }
        }

        private static long AddEmptyLineLenght(ref long index)
        {
            return index + 1;
        }

        private static long GetCurrnetStreamLocationIndex(long index, string line)
        {
            var newLineCharCount = 1;
            return index + line.Length + newLineCharCount;
        }

        private Box? CreateBoxFromHeader(StreamReader reader, ref long index)
        {
            Box? box = null;

            string? header = reader.ReadLine();

            if (string.IsNullOrWhiteSpace(header))
            {
                return null;
            }

            index = GetCurrnetStreamLocationIndex(index, header);

            if (header.Contains("HDR"))
            {
                string[] operands = Regex.Split(header.Trim(), @"\s+");

                //ToDO - Add more validation here to verify that the values are correct
                if (operands.Length == 3)
                {
                    box = new()
                    {
                        SupplierIdentifier = operands[1],
                        Identifier = operands[2],
                        Contents = new List<Content>()
                    };
                }
            }
            else
            {
                throw new Exception($"Invalid box Header: {header}");
            }

            return box;
        }

        private void ProcessChunk(Box box)
        {
            //ToDo - Send to some tasker or queue or at minimum create multiple processes to run this in parallel
            var boxHeader = new BoxHeader()
            {
                Identifier = box.Identifier,
                SupplierIdentifier = box.SupplierIdentifier
            };

            _unitOfWork.BoxHeaderRepository.Insert(boxHeader);            
            _unitOfWork.Save();

            box.Contents.ForEach(_ => boxHeader.BoxLines.Add(new BoxLine()
            {
                PoNumber = _.PoNumber,
                Isbn = _.Isbn,
                Quantity = _.Quantity,
                BoxHeaderId = boxHeader.Id
            }));

            _unitOfWork.Save();

        }
    }
}
