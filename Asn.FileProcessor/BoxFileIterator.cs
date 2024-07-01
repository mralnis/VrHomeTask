using Asn.FileProcessor.Data;
using System.Collections;
using System.Text.RegularExpressions;

namespace Asn.FileProcessor
{
    internal class BoxFileIterator : IEnumerable<Box>
    {
        private const string HeaderIndicator = "HDR";

        private readonly string _filePath;
        private long _index;

        public BoxFileIterator(string filePath)
        {
            _filePath = filePath;
        }

        public IEnumerator<Box> GetEnumerator()
        {
            _index = 0;

            while (true)
            {
                if (_index == -1)
                {
                    yield break;
                }

                using (FileStream fs = new(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    fs.Position = _index;

                    using (StreamReader reader = new(fs))
                    {
                        var box = CreateBoxFromHeader(reader);

                        if (box == null)
                        {
                            //ToDo - Get a business decision on what to do when a box header is not found
                            throw new Exception("Box header not found");
                        }

                        AddBoxContent(box, reader);

                        if (box.Contents.Count == 0)
                        {
                            //ToDo - Get a business decision on what to do when a box content is not found
                            throw new Exception("Box content not found");
                        }

                        fs.Close();

                        yield return box;
                    }
                }
            }
        }
        private Box? CreateBoxFromHeader(StreamReader reader)
        {
            Box? box = null;

            string? header = reader.ReadLine();

            if (header == null)
            {
                return null;
            }

            SetCurrentStreamLocationIndex(header);

            if (header.Contains(HeaderIndicator))
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
        private void AddBoxContent(Box? box, StreamReader reader)
        {
            bool nextBoxFound = false;
            while (!nextBoxFound)
            {
                var contentLine = reader.ReadLine();

                if (contentLine == string.Empty)
                {
                    AddEmptyLineLengthToCurrentIndex();
                    continue;
                }

                bool isEndOfFile = contentLine == null;
                if (isEndOfFile)
                {
                    _index = -1;
                    return;
                }

                if (contentLine.Contains(HeaderIndicator))
                {
                    nextBoxFound = true;
                    continue;
                }
                else
                {
                    SetCurrentStreamLocationIndex(contentLine);
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
        private void AddEmptyLineLengthToCurrentIndex()
        {
            _index++;
        }
        private void SetCurrentStreamLocationIndex(string line)
        {
            var newLineCharCount = 1;
            _index = _index + line.Length + newLineCharCount;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
