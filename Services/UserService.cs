using AutoMapper;
using HotelAPI.DTOs;
using HotelAPI.Models;
using HotelAPI.Repositories;

namespace HotelAPI.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IMapper _mapper;
    private readonly IRoleRepository _roleRepository;

    public UserService(IUserRepository repository, IMapper mapper, IRoleRepository roleRepository)
    {
        _repository = repository;
        _mapper = mapper;
        _roleRepository = roleRepository;
    }

    public async Task<(List<UserDTO> Items, int TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken ct)
    {
        if (page < 1 || pageSize < 1 || (long)(page - 1) * pageSize > int.MaxValue)
            throw new InvalidOperationException("Некорректные параметры пагинации.");
        var entities = await _repository.GetAllAsync(page, pageSize, ct);
        var total = await _repository.CountAsync(ct);
        return (_mapper.Map<List<UserDTO>>(entities), total);
    }

    public async Task<UserDTO?> GetByIdAsync(int id, CancellationToken ct)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        return entity is null ? null : _mapper.Map<UserDTO>(entity);
    }

    public async Task<UserDTO?> CreateAsync(CreateUserDTO dto, CancellationToken ct)
    {
        var entity = _mapper.Map<User>(dto);
        if (!await ValidateAsync(entity, ct)) return null;
        await _repository.AddAsync(entity, ct);
        await _repository.SaveChangesAsync(ct);
        return await GetByIdAsync(entity.IdUser, ct);
    }

    public async Task<bool> UpdateAsync(int id, UpdateUserDTO dto, CancellationToken ct)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null) return false;
        _mapper.Map(dto, entity);
        if (!await ValidateAsync(entity, ct)) return false;
        _repository.Update(entity);
        await _repository.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> PatchAsync(int id, PatchUserDTO dto, CancellationToken ct)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null) return false;
        _mapper.Map(dto, entity);
        if (!await ValidateAsync(entity, ct)) return false;
        _repository.Update(entity);
        await _repository.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null) return false;
        if (await _repository.IsReferencedAsync(id, ct))
            throw new InvalidOperationException("Удаление невозможно: существуют связанные записи.");
        _repository.Remove(entity);
        await _repository.SaveChangesAsync(ct);
        return true;
    }

    private async Task<bool> ValidateAsync(User entity, CancellationToken ct)
    {
        var existing = await _repository.FindByLoginAsync(entity.Login, ct);
        if (existing is not null && existing.IdUser != entity.IdUser)
            throw new InvalidOperationException("Значение Login уже используется.");
        if (string.IsNullOrWhiteSpace(entity.Login) || entity.Login.Length > 50)
            throw new InvalidOperationException("Поле Login обязательно и должно содержать не более 50 символов.");
        if (string.IsNullOrWhiteSpace(entity.UserPassword) || entity.UserPassword.Length > 256)
            throw new InvalidOperationException("Поле UserPassword обязательно и должно содержать не более 256 символов.");
        if (entity.UserName?.Length > 100) throw new InvalidOperationException("Поле UserName: не более 100 символов.");
        if (await _roleRepository.GetByIdAsync(entity.RoleId, ct) is null) return false;
        return true;
    }
}
