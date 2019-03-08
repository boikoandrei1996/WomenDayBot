﻿using System;
using Newtonsoft.Json;

namespace WomenDay.Models
{
  public class Order
  {
    [JsonProperty(PropertyName = "id")]
    public Guid DocumentId { get; set; }
    [JsonProperty(PropertyName = "orderId")]
    public Guid OrderId { get; set; }
    public string OrderType { get; set; }
    public string OrderCategory { get; set; }
    public bool IsComplete { get; set; }
    public UserData UserData { get; set; }
    public DateTime RequestTime { get; set; }
    public string Comment { get; set; }
  }
}
