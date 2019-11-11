using System;

namespace CanisModels
{
    public class BaseModel
    {
        private DateTime? created;
        private DateTime? lastModify;

        public uint? ID { get; set; } = 0;
        public DateTime? Created { get => created; set { created = value; CreatoIl = value.ToString(); } }
        public DateTime? LastModify { get => lastModify; set { lastModify = value; UltimaModifica = value.ToString(); } }

        #region Secondary Properties
        public string CreatoIl { get; set; }
        public string UltimaModifica { get; set; }
        #endregion
    }
}