using System;
using System.ComponentModel.DataAnnotations;

namespace ClassroomSystem.Models
{
    public class PaginationModel
    {
        private const int MaxPageSize = 100;
        private const int MinPageSize = 5;
        private const string PageSizeErrorMessage = "Page size must be between 5 and 100";

        private int _pageSize = 10;
        private int _pageNumber = 1;

        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }

        [Range(MinPageSize, MaxPageSize, ErrorMessage = PageSizeErrorMessage)]
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < MinPageSize ? MinPageSize : value > MaxPageSize ? MaxPageSize : value;
        }

        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
        public int StartPage => Math.Max(1, PageNumber - 2);
        public int EndPage => Math.Min(TotalPages, PageNumber + 2);
        public int Skip => (PageNumber - 1) * PageSize;
        public int Take => PageSize;
        public int CurrentPage { get; set; }

        public PaginationModel(int currentPage, int totalItems, int pageSize = 10)
        {
            CurrentPage = currentPage;
            TotalItems = totalItems;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            PageNumber = currentPage;
        }

        public int StartItem => ((CurrentPage - 1) * PageSize) + 1;
        public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);
    }
} 