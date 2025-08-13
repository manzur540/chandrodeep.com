using Member_App.Models;

namespace Member_App.Services
{
    public interface IMemberService
    {
        Task<List<Member>> GetAllMembersAsync();
        Task<Member?> GetMemberByIdAsync(int id);
        Task<string> SaveMemberAsync(Member member, IFormFile? file);
        Task<string> UpdateMemberAsync(int id, Member member, IFormFile? file);
        Task DeleteMemberAsync(int id);
    }
}
