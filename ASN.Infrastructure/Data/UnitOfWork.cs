using ASN.Infrastructure.Data.Models;
using ASN.Infrastructure.Data.Repos;

namespace ASN.Infrastructure.Data
{
    public class UnitOfWork(AsnContext asnContext) : IDisposable
    {
        private readonly AsnContext context = asnContext;

        private GenericRepository<BoxHeader> _boxHeaderRepository;
        private GenericRepository<BoxLine> _boxLineRepository;

        public GenericRepository<BoxHeader> BoxHeaderRepository
        {
            get
            {

                if (_boxHeaderRepository == null)
                {
                    _boxHeaderRepository = new GenericRepository<BoxHeader>(context);
                }
                return _boxHeaderRepository;
            }
        }

        public GenericRepository<BoxLine> BoxLineRepository
        {
            get
            {

                if (this._boxLineRepository == null)
                {
                    this._boxLineRepository = new GenericRepository<BoxLine>(context);
                }
                return _boxLineRepository;
            }
        }

        public void Save()
        {
            context.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
