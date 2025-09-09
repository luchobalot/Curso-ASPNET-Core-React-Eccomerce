using System.Linq.Expressions;

namespace Ecommerce.Application.Specifications;

public class ISpecification<T>
{
    public Expression<Func<T, bool>>? Criteria { get; }
    public List<Expression<Func<T, object>>>? Includes { get; }
    public Expression<Func<T, object>> OrderBy { get; }
    public Expression<Func<T, object>> OrderByDescending { get; }
    public int Take { get; }
    public int Skip { get; }
    public bool IsPagingEnabled { get;}

}