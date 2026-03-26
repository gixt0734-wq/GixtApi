using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Application.Interfaces;
using GixtApiBackend.Domain.Entities;

namespace GixtApiBackend.Application.UseCases.Locations
{ 
   public class DeleteLocation
    {
        private readonly ILocationRepository _repository;

        public DeleteLocation(ILocationRepository repository)
        {
            _repository = repository;
        }

        public async Task Execute(Guid id)
        {
            await _repository.DeleteLocationAsync(id);
        }
    }
}
