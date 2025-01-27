using AdministrationAPI.Data;
using AdministrationAPI.DTOs;
using AdministrationAPI.DTOs.Transaction;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using AdministrationAPI.Models.Transaction;


namespace AdministrationAPI.Services.Transaction
{
    public class TransactionService : ITransactionService
    {
        private readonly IMapper _mapper;
        private readonly DBContext _context;

        public TransactionService(IMapper mapper, DBContext context)
        {
            _mapper = mapper;
            _context = context;

        }


        public async Task<TransactionResponseDTO> GetTransactions(TransactionQueryOptions options)
        {

            // Throw error if pageNumber or pageSize is less than 1
            if (options.PageNumber < 1 || options.PageSize < 1)
            {
                throw new Exception("PageNumber and PageSize must be greater than or equal to 1.");
            }

            var transactions = _context.Transactions.AsQueryable();

            // Filter transactions
            if (options.DateTimeStart == null && options.DateTimeEnd != null) options.DateTimeStart = options.DateTimeEnd;
            if (options.DateTimeEnd == null && options.DateTimeStart != null) options.DateTimeEnd = options.DateTimeStart;

            if (options.DateTimeStart != null && options.DateTimeEnd != null)
            {
                transactions = transactions.Where(t => t.DateTime >= options.DateTimeStart && t.DateTime <= options.DateTimeEnd);
            }

            if (!string.IsNullOrEmpty(options.Recipient))
            {
                transactions = transactions.Where(t => t.Recipient.ToLower() == options.Recipient.ToLower());
            }

            if (options.MinAmount == null && options.MaxAmount != null) options.MinAmount = options.MaxAmount;
            if (options.MaxAmount == null && options.MinAmount != null) options.MaxAmount = options.MinAmount;

            if (options.MinAmount != null && options.MaxAmount != null)
            {
                transactions = transactions.Where(t => t.Amount >= options.MinAmount && t.Amount <= options.MaxAmount);
            }

            if (options.Status != null)
            {
                transactions = transactions.Where(t => t.Status == options.Status);
            }

            // Sort transactions
            if (options.SortingOptions != null)
            {
                if (options.SortingOptions == SortingOptions.Amount)
                    transactions = (options.Ascending == true) ? transactions.OrderBy(t => t.Amount) : transactions.OrderByDescending(t => t.Amount);
                else if (options.SortingOptions == SortingOptions.Recipient)
                    transactions = (options.Ascending == true) ? transactions.OrderBy(t => t.Recipient) : transactions.OrderByDescending(t => t.Recipient);
                else if (options.SortingOptions == SortingOptions.Status)
                    transactions = (options.Ascending == true) ? transactions.OrderBy(t => t.Status) : transactions.OrderByDescending(t => t.Status);
                else
                    transactions = (options.Ascending == true) ? transactions.OrderBy(t => t.DateTime) : transactions.OrderByDescending(t => t.DateTime);
            }

            var response = new TransactionResponseDTO();

            // Set default values for pageNumber and pageSize
            int pageNumber = options.PageNumber ?? 1;
            int pageSize = options.PageSize ?? await transactions.CountAsync();

            // Calculate skip count based on pageNumber and pageSize
            int skipCount = (pageNumber - 1) * pageSize;

            // Apply pagination
            transactions = transactions.Skip(skipCount).Take(pageSize);

            response.Transactions = await transactions.Select(transaction => _mapper.Map<TransactionDTO>(transaction)).ToListAsync();
            response.Pages = (int)Math.Ceiling((double)await transactions.CountAsync() / pageSize);
            response.CurrentPage = pageNumber;

            return response;
        }

        public async Task<TransactionDetailsDTO> GetTransactionByID(int id)
        {
            if (id < 1) throw new Exception("You have specified an invalid id.");
            var dbTransaction = await _context.Transactions.FirstOrDefaultAsync(transaction => transaction.Id == id);
            if (dbTransaction is null) throw new Exception("No transaction corresponds to the given id.");

            return _mapper.Map<TransactionDetailsDTO>(dbTransaction);
        }
    }
}