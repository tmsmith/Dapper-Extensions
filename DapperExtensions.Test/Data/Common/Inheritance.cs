using DapperExtensions.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DapperExtensions.Test.Data.Common
{
    public enum OrderType
    {
        Product = 1,
        Service
    }

    #region Data Classes

    public abstract class Order<TItem> where TItem : OrderItem
    {
        protected Order(OrderType orderType)
        {
            OrderType = orderType;
        }

        public long Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public IEnumerable<TItem> Items { get; set; }
        public double OrderTotal { get { return Items.Any() ? Items.Select(i => i.ItemTotal).Sum() : 0; } }
        public OrderType OrderType { get; }
    }

    public abstract class OrderItem
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public double Price { get; set; }
        public double ItemTotal { get; set; }
    }

    public class ProductOrder : Order<ProductItem>
    {
        public ProductOrder() : base(OrderType.Product) { }

        public DateTime DeliveryDate { get; set; }
    }

    public class ProductItem : OrderItem
    {
        public string EAN { get; set; }
        public int Quantity { get; set; }
    }

    public class ServiceOrder : Order<ServiceItem>
    {
        public ServiceOrder() : base(OrderType.Service) { }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class ServiceItem : OrderItem
    {
        public string ServiceDescription { get; set; }
    }
    #endregion

    #region ClassMappers
    public abstract class OrderMap<T, TItem> : ClassMapper<T>
        where T : Order<TItem>
        where TItem : OrderItem
    {
        protected OrderMap()
        {
            TableName = "Order";

            Map(t => t.Id).Column("Id").Key(KeyType.Assigned);
            Map(t => t.OrderTotal).Ignore();
            Map(t => t.Items).Ignore();
            AutoMap();

            ReferenceMap(t => t.Items).Reference<TItem>((item, order) => item.OrderId == order.Id);
        }
    }

    public abstract class OrderItemMap<T> : ClassMapper<T> where T : OrderItem
    {
        protected OrderItemMap()
        {
            Map(t => t.Id).Column("Id").Key(KeyType.Assigned);
            Map(t => t.OrderId).Column("OrderId").Key(KeyType.ForeignKey);
            AutoMap();
        }
    }

    public class ProductOrderMap : OrderMap<ProductOrder, ProductItem>
    {
        public ProductOrderMap() : base()
        {
            AutoMap();
        }
    }

    public class ProductItemMap : OrderItemMap<ProductItem>
    {
        public ProductItemMap()
        {
            TableName = "ProductItem";
            AutoMap();
        }
    }

    public class ServiceOrderMap : OrderMap<ServiceOrder, ServiceItem>
    {
        public ServiceOrderMap()
        {
            AutoMap();
        }
    }

    public class ServiceItemMap : OrderItemMap<ServiceItem>
    {
        public ServiceItemMap()
        {
            TableName = "ServiceItem";
            AutoMap();
        }
    }
    #endregion
}
