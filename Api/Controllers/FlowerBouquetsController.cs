﻿using Api.Models;
using Api.Utils;
using Application.Exceptions;
using BusinessObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository;

namespace Api.Controllers;

[Route("api/v1/flower-bouquets")]
public class FlowerBouquetsController : BaseController
{
    private readonly IRepository<FlowerBouquet> _flowerRepository;
    private readonly IRepository<Category> _catgoryRepository;
    private readonly IRepository<Supplier> _supplierRepository;
    private readonly IRepository<OrderDetail> _oderDetailRepository;
                                                               
    public FlowerBouquetsController(IRepository<FlowerBouquet> flowerRepository,
        IRepository<Category> catgoryRepository,
        IRepository<Supplier> supplierRepository,
        IRepository<OrderDetail> oderDetailRepository)
    {
        _flowerRepository = flowerRepository;
        _catgoryRepository = catgoryRepository;
        _supplierRepository = supplierRepository;
        _oderDetailRepository = oderDetailRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetFlowerBouquets()
    {
        return Ok(await _flowerRepository.ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> CreateFlowerBouquet([FromBody] CreateFlowerBouquet req)
    {
        FlowerBouquet entity = Mapper.Map(req, new FlowerBouquet());
        entity.FlowerBouquetId = await GetId();
        await ValidateNavigations(entity);
        await _flowerRepository.CreateAsync(entity);
        return StatusCode(StatusCodes.Status201Created);
    }

    private async Task<int> GetId()
    {
        var flower = (await _flowerRepository.ToListAsync()).OrderByDescending(u => u.FlowerBouquetId).FirstOrDefault();
        return flower == null ? 1 : (flower.FlowerBouquetId + 1);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetFlowerBouquet(int id)
    {
        var target = await _flowerRepository.FoundOrThrow(f => f.FlowerBouquetId == id, new NotFoundException());
        return Ok(target);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFlowerBouquet(int id, [FromBody] UpdateFlowerBouquet req)
    {
        var target = await _flowerRepository.FoundOrThrow(f => f.FlowerBouquetId == id, new NotFoundException());
        FlowerBouquet entity = Mapper.Map(req, target);
        await ValidateNavigations(entity);
        await _flowerRepository.UpdateAsync(entity);
        return StatusCode(StatusCodes.Status204NoContent);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFlowerBouquet(int id)
    {
        var isFree = await _oderDetailRepository.FirstOrDefaultAsync(od => od.FlowerBouquetId == id) == null;
        if (!isFree)
        {
            throw new BadRequestException("Flower had been ordered, cannot delete");
        }
        var target = await _flowerRepository.FoundOrThrow(f => f.FlowerBouquetId == id, new NotFoundException());
        await _flowerRepository.DeleteAsync(target);
        return StatusCode(StatusCodes.Status204NoContent);
    }

    private async Task ValidateCategory(int id)
    {
        await _catgoryRepository.FoundOrThrow(c => c.CategoryId == id, new BadRequestException("Category not found"));
    }

    private async Task ValidateSupplier(int id)
    {
        await _supplierRepository.FoundOrThrow(s => s.SupplierId == id, new BadRequestException("Supplier not found"));
    }

    private async Task ValidateNavigations(FlowerBouquet entity)
    {
        await ValidateCategory(entity.CategoryId);
        if (entity.SupplierId != null)
        {
            await ValidateSupplier((int)entity.SupplierId);
        }
    }
}
