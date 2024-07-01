using Asn.FileProcessor.Data;
using ASN.Infrastructure.Data;
using ASN.Infrastructure.Data.Models;

namespace Asn.FileProcessor
{
    public class FileProcessor(UnitOfWork unitOfWork)
    {
        private readonly UnitOfWork _unitOfWork = unitOfWork;

        public void Process(string filePath)
        {
            //If we have more then one file type/structure this is the place we would add more logic to process the files differently
            IEnumerable<Box> boxIterator = new BoxFileIterator(filePath);

            foreach (Box box in boxIterator)
            {
                ProcessChunk(box);
            }           
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
