#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.Annotations;
using Lambda.GraphQL.Attributes;

namespace Lambda.GraphQL.Examples;

// Union type example
[GraphQLUnion("SearchResult", "Product", "User", "Order")]
public class SearchResult
{
    // This class serves as a marker for the union type
    // Actual resolution happens in Lambda functions
}

// Interface example with AWS scalars
[GraphQLType("Node", Kind = GraphQLTypeKind.Interface)]
public interface INode
{
    [GraphQLField(Description = "Unique identifier")]
    Guid Id { get; }
    
    [GraphQLField(Description = "Creation timestamp")]
    DateTime CreatedAt { get; }
}

// Object implementing interface with AWS scalars
[GraphQLType("User", Description = "A user in the system")]
[GraphQLAuthDirective(AuthMode.UserPools)]
public class User : INode
{
    [GraphQLField(Description = "Unique user identifier")]
    public Guid Id { get; set; }
    
    [GraphQLField(Description = "User creation timestamp")]
    public DateTime CreatedAt { get; set; }
    
    [GraphQLField(Description = "User's email address")]
    public System.Net.Mail.MailAddress Email { get; set; } = null!;
    
    [GraphQLField(Description = "User's profile URL")]
    public Uri? ProfileUrl { get; set; }
    
    [GraphQLField(Description = "User's IP address")]
    public System.Net.IPAddress? LastLoginIp { get; set; }
    
    [GraphQLField(Description = "User metadata as JSON")]
    public System.Text.Json.JsonElement? Metadata { get; set; }
    
    [GraphQLField(Description = "User's birth date")]
    public DateOnly? BirthDate { get; set; }
    
    [GraphQLField(Description = "Preferred notification time")]
    public TimeOnly? NotificationTime { get; set; }
    
    [GraphQLField(Description = "Account creation timestamp (Unix seconds)")]
    [GraphQLTimestamp]
    public long CreatedAtTimestamp { get; set; }
}

// Object implementing interface
[GraphQLType("Order", Description = "An order in the system")]
public class Order : INode
{
    [GraphQLField(Description = "Unique order identifier")]
    public Guid Id { get; set; }
    
    [GraphQLField(Description = "Order creation timestamp")]
    public DateTime CreatedAt { get; set; }
    
    [GraphQLField(Description = "Order total amount")]
    public decimal Total { get; set; }
    
    [GraphQLField(Description = "Order status")]
    public OrderStatus Status { get; set; }
}

// Enum with AWS auth directive
[GraphQLType("OrderStatus", Description = "Order status enumeration")]
[GraphQLAuthDirective(AuthMode.UserPools)]
public enum OrderStatus
{
    [GraphQLEnumValue(Description = "Order is pending")]
    Pending,
    
    [GraphQLEnumValue(Description = "Order is being processed")]
    Processing,
    
    [GraphQLEnumValue(Description = "Order has been shipped")]
    Shipped,
    
    [GraphQLEnumValue(Description = "Order has been delivered")]
    Delivered,
    
    [GraphQLEnumValue(Description = "Order was cancelled")]
    Cancelled
}

// Lambda functions demonstrating advanced features
public class AdvancedFunctions
{
    [LambdaFunction]
    [GraphQLQuery("search", Description = "Search for products, users, or orders", ReturnType = "SearchResult")]
    [GraphQLResolver(DataSource = "SearchLambda")]
    public async Task<List<object>> Search(
        [GraphQLArgument(Description = "Search term")] string term,
        [GraphQLArgument(Description = "Maximum results to return")] int limit)
    {
        // Implementation would return mixed types for union resolution
        await Task.Delay(1); // Placeholder
        return new List<object>();
    }

    [LambdaFunction]
    [GraphQLQuery("getUser", Description = "Get a user by ID")]
    [GraphQLResolver(DataSource = "UserLambda")]
    [GraphQLAuthDirective(AuthMode.UserPools)]
    public async Task<User> GetUser(
        [GraphQLArgument(Description = "User ID")] Guid id)
    {
        // Implementation would fetch user
        await Task.Delay(1); // Placeholder
        return new User
        {
            Id = id,
            CreatedAt = DateTime.UtcNow,
            Email = new System.Net.Mail.MailAddress("user@example.com")
        };
    }

    [LambdaFunction]
    [GraphQLMutation("createUser", Description = "Create a new user")]
    [GraphQLResolver(DataSource = "UserLambda")]
    [GraphQLAuthDirective(AuthMode.UserPools, CognitoGroups = "admin")]
    public async Task<User> CreateUser(
        [GraphQLArgument(Description = "User creation input")] CreateUserInput input)
    {
        // Implementation would create user
        await Task.Delay(1); // Placeholder
        return new User
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Email = new System.Net.Mail.MailAddress(input.Email)
        };
    }

    [LambdaFunction]
    [GraphQLSubscription("orderStatusChanged", Description = "Subscribe to order status changes")]
    [GraphQLResolver(DataSource = "OrderSubscriptionLambda")]
    [GraphQLAuthDirective(AuthMode.UserPools)]
    public async Task<Order> OrderStatusChanged(
        [GraphQLArgument(Description = "Order ID to watch")] Guid orderId)
    {
        // Implementation would set up subscription
        await Task.Delay(1); // Placeholder
        return new Order { Id = orderId, CreatedAt = DateTime.UtcNow, Total = 0, Status = OrderStatus.Pending };
    }
}

// Input type with AWS scalars
[GraphQLType("CreateUserInput", Kind = GraphQLTypeKind.Input)]
public class CreateUserInput
{
    [GraphQLField(Description = "User's email address")]
    public string Email { get; set; } = string.Empty;
    
    [GraphQLField(Description = "User's birth date")]
    public DateOnly? BirthDate { get; set; }
    
    [GraphQLField(Description = "User metadata as JSON")]
    public System.Text.Json.JsonElement? Metadata { get; set; }
}
