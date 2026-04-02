//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using GixtApiBackend.Application.Interfaces;
//using GixtApiBackend.Domain.Entities;

//namespace GixtApiBackend.Application.UseCases.Cost
//{
//    public class GetCost
//    {
//        private readonly ICostRepository _repo;
//        public GetCost(ICostRepository repo)
//        {
//            _repo = repo;
//        }
//        public async Task<IEnumerable<Costs>> Execute()
//        {

//            return await _repo.GetAllCostAsync();
//        }

//    }
//}
