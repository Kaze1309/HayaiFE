using HayaiFE.Data;
using HayaiFE.Models;
using Microsoft.EntityFrameworkCore;

namespace HayaiFE.Services
{
    public class ExamBlockService
    {
        private readonly ApplicationDbContext _context;

        public ExamBlockService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Add a new block
        public async Task AddBlockAsync(BlockData block)
        {
            _context.BlocksInfo.Add(block);
            await _context.SaveChangesAsync();
        }

        // Get block by BlockNumber
        public async Task<BlockData?> GetBlockByNumberAsync(int blockNumber)
        {
            return await _context.BlocksInfo.FirstOrDefaultAsync(b => b.BlockNumber == blockNumber);
        }

        // Get all blocks
        public async Task<List<BlockData>> GetAllBlocksAsync()
        {
            return await _context.BlocksInfo.ToListAsync();
        }

        // Update a block's subject, branch, teacher, etc.

    }
}
