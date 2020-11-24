using System;

namespace Domain.Contract
{
    public interface IResponseData
    {
        public int Id { get; set; }
        public String Name { get; set; }
    }
}