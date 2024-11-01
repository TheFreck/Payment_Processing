﻿using Machine.Specifications;
using Microsoft.Cci;
using Moq;
using Payment_Processing.Server.DTO;
using Payment_Processing.Server.Repos;
using Payment_Processing.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Payment_Processing.Server.Services.ProductService;
using It = Machine.Specifications.It;

namespace Payment_Processing.Specs
{
    public class With_ProductRepo_Setup
    {
        Establish context = () =>
        {
            productRepoMock = new Mock<IProductRepo>();
            itemRepoMock = new Mock<IItemRepo>();
            accountRepoMock = new Mock<IAccountRepo>();
            loginServiceMock = new Mock<ILoginService>();
            account1Id = Guid.NewGuid().ToString();
            account2Id = Guid.NewGuid().ToString();
            name1 = "Carl";
            name2 = "Jane";
            email1 = "carl@email.com";
            email2 = "jane@email.com";
            password1 = "password1";
            password2 = "password2";
            permission1 = new List<AccountPermissions>
            {
                new AccountPermissions
                {
                    Type = PermissionType.Admin,
                    Token = "adminToken1"
                },
                new AccountPermissions
                {
                    Type = PermissionType.User,
                    Token = "userToken1"
                }
            };
            permission2 = new List<AccountPermissions>
            {
                new AccountPermissions
                {
                    Type = PermissionType.User,
                    Token = "userToken1"
                }
            };
            account1 = new Account(name1, email1, password1, email1, permission1);
            account2 = new Account(name2, email2, password2, email2, permission2);
            account1.Token = "token1";
            account2.Token = "token2";
            product1Id = Guid.NewGuid().ToString();
            product2Id = Guid.NewGuid().ToString();
            product1Name = "hat";
            product2Name = "shirt";
            product1Desc = "it is worn on your head";
            product2Desc = "it is worn on your torso";
            product1Price = 25.75;
            product2Price = 74.99;
        };


        protected static Mock<IProductRepo> productRepoMock;
        protected static Mock<IItemRepo> itemRepoMock;
        protected static Mock<IAccountRepo> accountRepoMock;
        protected static Mock<ILoginService> loginServiceMock;
        protected static string account1Id;
        protected static string account2Id;
        protected static string name1;
        protected static string name2;
        protected static string email1;
        protected static string email2;
        protected static string password1;
        protected static string password2;
        protected static List<AccountPermissions> permission1;
        protected static List<AccountPermissions> permission2;
        protected static Account account1;
        protected static Account account2;
        protected static string product1Id;
        protected static string product2Id;
        protected static string product1Name;
        protected static string product2Name;
        protected static string product1Desc;
        protected static string product2Desc;
        protected static double product1Price;
        protected static double product2Price;
    }

    public class When_Creating_A_New_Product : With_ProductRepo_Setup
    {
        Establish context = () =>
        {
            loginServiceMock.Setup(l => l.ValidatePermissionsAsync(Moq.It.IsAny<Account>(), PermissionType.Admin)).ReturnsAsync(true);
            inputs = new List<(string name, string id, string desccription, double price)>
            {
                new (product1Name,product1Id,product1Desc,product1Price),
                new (product2Name,product2Id,product2Desc,product2Price)
            };
            storeFront = new ProductService(productRepoMock.Object,itemRepoMock.Object, accountRepoMock.Object, loginServiceMock.Object);
            expectations = new List<Product>
            {
                new Product()
                {
                    Name = product1Name,
                    ProductDescription = product1Desc,
                    ProductId = product1Id,
                    Price = product1Price
                },
                new Product()
                {
                    Name= product2Name,
                    ProductDescription = product2Desc,
                    ProductId = product2Id,
                    Price = product2Price
                }
            };
            outcomes = new List<Product>();
        };

        Because of = () => 
        {
            for(var i = 0; i < inputs.Count; i++)
            {
                outcomes.Add(storeFront.CreateProductAsync(account1.Username, inputs[i].name, inputs[i].desccription, inputs[i].price).GetAwaiter().GetResult());
            }
        };

        It Should_Return_A_Fully_Formed_Product = () =>
        {
            for(var i=0; i<expectations.Count; i++)
            {
                outcomes[i].Name.ShouldEqual(expectations[i].Name);
                outcomes[i].ProductId.ShouldNotEqual(Guid.Empty.ToString());
                outcomes[i].ProductDescription.ShouldEqual(expectations[i].ProductDescription);
                outcomes[i].Price.ShouldEqual(expectations[i].Price);
            }
        };

        It Should_Validate_Admin_Permission = () =>
        {
            loginServiceMock.Verify(l => l.ValidatePermissionsAsync(Moq.It.IsAny<Account>(), Moq.It.IsAny<PermissionType>()), Times.Exactly(inputs.Count));
        };

        It Should_Persist_New_Product = () =>
        {
            productRepoMock.Verify(p => p.CreateAsync(Moq.It.IsAny<Product>()), Times.Exactly(inputs.Count));
        };

        private static IProductService storeFront;
        private static List<(string name, string id, string desccription, double price)> inputs;
        private static List<Product> expectations;
        private static List<Product> outcomes;
    }

    public class When_Modifying_The_Description_Of_A_Product : With_ProductRepo_Setup
    {
        Establish context = () =>
        {
            product1NewDesc = "This product covers your head";
            product2NewDesc = "This is gonna fit just right";
            product1 = new Product
            {
                Name = product1Name,
                ProductDescription = product1NewDesc,
                ProductId = product1Id,
                Price = product1Price
            };
            product2 = new Product
            {
                Name = product2Name,
                ProductDescription = product2NewDesc,
                ProductId = product2Id,
                Price = product2Price
            };
            loginServiceMock.Setup(l => l.ValidatePermissionsAsync(Moq.It.IsAny<Account>(), PermissionType.Admin)).ReturnsAsync(true);
            productRepoMock.Setup(p => p.GetByNameAsync(product1Name)).ReturnsAsync(product1);
            productRepoMock.Setup(p => p.GetByNameAsync(product2Name)).ReturnsAsync(product2);
            storeFront = new ProductService(productRepoMock.Object,itemRepoMock.Object, accountRepoMock.Object, loginServiceMock.Object);
            inputs = new List<(string name, string newDescription)>
            {
                new (product1Name,product1NewDesc),
                new (product2Name,product2NewDesc)
            };
            expectations = new List<Product>
            {
                product1,
                product2
            };
            outcomes = new List<Product>();
        };

        Because of = () =>
        {
            for (var i = 0; i < inputs.Count; i++)
            {
                outcomes.Add(storeFront.ModifyDescriptionAsync(account1.Username, inputs[i].name,product1NewDesc).GetAwaiter().GetResult());
            }
        };

        It Should_Validate_Admin_Permission = () =>
        {
            loginServiceMock.Verify(l => l.ValidatePermissionsAsync(Moq.It.IsAny<Account>(), Moq.It.IsAny<PermissionType>()), Times.Exactly(expectations.Count));
        };

        It Should_Return_Modified_Product = () =>
        {
            for(var i = 0;i < expectations.Count; i++)
            {
                outcomes[i].Name.ShouldEqual(expectations[i].Name);
                outcomes[i].ProductDescription.ShouldEqual(expectations[i].ProductDescription);
            }
        };

        It Should_Retrieve_Product_From_Repo = () =>
        {
            for (var i = 0; i < expectations.Count; i++)
            {
                productRepoMock.Verify(p => p.GetByNameAsync(expectations[i].Name), Times.Once);
            }
        };

        It Should_Persist_Modified_Product = () =>
        {
            for (var i = 0; i < expectations.Count; i++)
            {
                productRepoMock.Verify(p => p.UpdateAsync(Moq.It.IsAny<Product>()), Times.Exactly(expectations.Count));
            }
        };

        private static ProductService storeFront;
        private static string product1NewDesc;
        private static string product2NewDesc;
        private static Product product1;
        private static Product product2;
        private static List<(string name, string newDescription)> inputs;
        private static List<Product> expectations;
        private static List<Product> outcomes;
    }

    public class When_Modifying_The_Name_Of_A_Product : With_ProductRepo_Setup
    {
        Establish context = () =>
        {
            product1NewName = "cap";
            product2NewName = "top";
            product1 = new Product
            {
                Name = product1Name,
                ProductDescription = product1NewName,
                ProductId = product1Id,
                Price = product1Price
            };
            product2 = new Product
            {
                Name = product2Name,
                ProductDescription = product2NewName,
                ProductId = product2Id,
                Price = product2Price
            };
            newProduct1 = product1;
            newProduct2 = product2;
            newProduct1.Name = product1NewName;
            newProduct2.Name = product2NewName;
            loginServiceMock.Setup(l => l.ValidatePermissionsAsync(Moq.It.IsAny<Account>(), PermissionType.Admin)).ReturnsAsync(true);
            productRepoMock.Setup(p => p.GetByNameAsync(product1Name)).ReturnsAsync(product1);
            productRepoMock.Setup(p => p.GetByNameAsync(product2Name)).ReturnsAsync(product2);
            productService = new ProductService(productRepoMock.Object,itemRepoMock.Object, accountRepoMock.Object, loginServiceMock.Object);
            inputs = new List<(string name, string newName)>
            {
                new (product1Name,product1NewName),
                new (product2Name,product2NewName)
            };
            expectations = new List<Product>
            {
                newProduct1,
                newProduct2
            };
            outcomes = new List<Product>();
        };

        Because of = () =>
        {
            for (var i = 0; i < inputs.Count; i++)
            {
                outcomes.Add(productService.ModifyNameAsync(account1.Username, inputs[i].name, inputs[i].newName).GetAwaiter().GetResult());
            }
        };

        It Should_Validate_Admin_Permission = () =>
        {
            loginServiceMock.Verify(l => l.ValidatePermissionsAsync(Moq.It.IsAny<Account>(), Moq.It.IsAny<PermissionType>()), Times.Exactly(expectations.Count));
        };

        It Should_Return_Modified_Product = () =>
        {
            for (var i = 0; i < expectations.Count; i++)
            {
                outcomes[i].Name.ShouldEqual(expectations[i].Name);
                outcomes[i].ProductDescription.ShouldEqual(expectations[i].ProductDescription);
            }
        };

        It Should_Retrieve_Product_From_Repo = () =>
        {
            for (var i = 0; i < expectations.Count; i++)
            {
                productRepoMock.Verify(p => p.GetByNameAsync(inputs[i].name), Times.Once);
            }
        };

        It Should_Persist_Modified_Product = () => productRepoMock.Verify(p => p.UpdateAsync(Moq.It.IsAny<Product>()), Times.Exactly(expectations.Count));

        private static IProductService productService;
        private static string product1NewName;
        private static string product2NewName;
        private static Product product1;
        private static Product product2;
        private static Product newProduct1;
        private static Product newProduct2;
        private static List<(string name, string newName)> inputs;
        private static List<Product> expectations;
        private static List<Product> outcomes;
    }

    public class When_Modifying_The_Price_Of_A_Product : With_ProductRepo_Setup
    {
        Establish context = () =>
        {
            product1NewPrice = 199.99;
            product2NewPrice = 34.75;
            product1 = new Product
            {
                Name = product1Name,
                ProductDescription = product1Desc,
                ProductId = product1Id,
                Price = product1Price
            };
            product2 = new Product
            {
                Name = product2Name,
                ProductDescription = product2Desc,
                ProductId = product2Id,
                Price = product2Price
            };
            loginServiceMock.Setup(l => l.ValidatePermissionsAsync(Moq.It.IsAny<Account>(), PermissionType.Admin)).ReturnsAsync(true);
            productRepoMock.Setup(p => p.GetByNameAsync(product1Name)).ReturnsAsync(product1);
            productRepoMock.Setup(p => p.GetByNameAsync(product2Name)).ReturnsAsync(product2);
            productRepoMock.Setup(p => p.UpdateAsync(Moq.It.IsAny<Product>()));
            productService = new ProductService(productRepoMock.Object, itemRepoMock.Object, accountRepoMock.Object, loginServiceMock.Object);
            inputs = new List<(string name, double price)>
            {
                new (product1Name,product1NewPrice),
                new (product2Name,product2NewPrice)
            };
            expectations = new List<Product>
            {
                product1,
                product2
            };
            outcomes = new List<Product>();
        };

        Because of = () =>
        {
            for (var i = 0; i < inputs.Count; i++)
            {
                outcomes.Add(productService.ModifyPriceAsync(account1.Username, inputs[i].name, inputs[i].price).GetAwaiter().GetResult());
            }
        };

        It Should_Validate_Admin_Permission = () =>
        {
            loginServiceMock.Verify(l => l.ValidatePermissionsAsync(Moq.It.IsAny<Account>(), Moq.It.IsAny<PermissionType>()), Times.Exactly(expectations.Count));
        };

        It Should_Return_Modified_Product = () =>
        {
            for (var i = 0; i < expectations.Count; i++)
            {
                outcomes[i].Name.ShouldEqual(expectations[i].Name);
                outcomes[i].ProductDescription.ShouldEqual(expectations[i].ProductDescription);
                outcomes[i].Price.ShouldEqual(expectations[i].Price);
            }
        };

        It Should_Retrieve_Product_From_Repo = () =>
        {
            for (var i = 0; i < expectations.Count; i++)
            {
                productRepoMock.Verify(p => p.GetByNameAsync(expectations[i].Name), Times.Once);
            }
        };

        It Should_Persist_Modified_Product = () =>
        {
            for (var i = 0; i < expectations.Count; i++)
            {
                productRepoMock.Verify(p => p.UpdateAsync(Moq.It.IsAny<Product>()), Times.Exactly(expectations.Count));
            }
        };

        private static ProductService productService;
        private static double product1NewPrice;
        private static double product2NewPrice;
        private static Product product1;
        private static Product product2;
        private static List<(string name, double price)> inputs;
        private static List<Product> expectations;
        private static List<Product> outcomes;
    }

    public class When_Getting_All_Products : With_ProductRepo_Setup
    {
        Establish context = () =>
        {
            products = new List<Product>
            {
                new Product
                {
                    Name = product1Name,
                    Price = product1Price,
                    ProductDescription = product1Desc,
                    ProductId = product1Id,
                },
                new Product
                {
                    Name= product2Name,
                    Price = product2Price,
                    ProductDescription = product2Desc,
                    ProductId = product2Id,
                }
            };
            productRepoMock.Setup(p => p.GetAllProductsAsync()).ReturnsAsync(products);
            productService = new ProductService(productRepoMock.Object, itemRepoMock.Object, accountRepoMock.Object, loginServiceMock.Object);
            expectations = products;
            outcomes = new List<Product>();
        };

        Because of = () => outcomes = productService.GetAllAsync().GetAwaiter().GetResult().ToList();

        It Should_Return_All_Products_From_Repo = () =>
        {
            for (var i = 0; i < expectations.Count; i++) 
            {
                outcomes[i].Name.ShouldEqual(expectations[i].Name);
                outcomes[i].ProductDescription.ShouldEqual(expectations[i].ProductDescription);
                outcomes[i].Price.ShouldEqual(expectations[i].Price);
                outcomes[i].Id.ShouldNotEqual(Guid.Empty.ToString());
            }
        };

        It Should_Call_Repo = () =>
        {
            for (var i = 0; i < expectations.Count; i++) 
            {
                productRepoMock.Verify(p => p.GetAllProductsAsync(),Times.Once);
            }
        };

        private static ProductService productService;
        private static List<Product> expectations;
        private static List<Product> outcomes;
        private static List<Product> products;
    }

    public class When_Getting_Product_By_Name : With_ProductRepo_Setup
    {
        Establish context = () =>
        {
            product1 = new Product
            {
                Name = product1Name,
                Price = product1Price,
                ProductDescription = product1Desc,
                ProductId = product1Id,
            };
            product2 = new Product
            {
                Name = product2Name,
                Price = product2Price,
                ProductDescription = product2Desc,
                ProductId = product2Id,
            };
            products = new List<Product>
            {
                product1,
                product2
            };
            productRepoMock.Setup(p => p.GetByNameAsync(product1Name)).ReturnsAsync(product1);
            productRepoMock.Setup(p => p.GetByNameAsync(product2Name)).ReturnsAsync(product2);
            productService = new ProductService(productRepoMock.Object, itemRepoMock.Object, accountRepoMock.Object, loginServiceMock.Object);
            expectations = products;
            outcomes = new List<Product>();
        };

        Because of = () => 
        {
            for(var i=0; i<products.Count; i++)
            {
                outcomes.Add(productService.GetByNameAsync(products[i].Name).GetAwaiter().GetResult());
            }
        };

        It Should_Return_Named_Product = () =>
        {
            for (var i = 0; i < expectations.Count; i++)
            {
                outcomes[i].Name.ShouldEqual(expectations[i].Name);
                outcomes[i].ProductDescription.ShouldEqual(expectations[i].ProductDescription);
                outcomes[i].Price.ShouldEqual(expectations[i].Price);
                outcomes[i].Id.ShouldNotEqual(Guid.Empty.ToString());
            }
        };

        It Should_Get_Product_From_Repo = () =>
        {
            for (var i = 0; i < expectations.Count; i++)
            {
                productRepoMock.Verify(p => p.GetByNameAsync(products[i].Name), Times.Once());
            }
        };

        private static ProductService productService;
        private static List<Product> expectations;
        private static List<Product> outcomes;
        private static Product product1;
        private static Product product2;
        private static List<Product> products;
    }

    public class When_Creating_An_Item_For_A_Product : With_ProductRepo_Setup
    {
        Establish context = () =>
        {
            colors = new List<string>
            {
                "Red","Green","Black"
            };
            sizes = new List<string>
            {
                "S","M","L","XL"
            };
            hatTypes = new List<string>
            {
                "Ballcap","Fedora"
            };
            hats = new List<Item>();
            for (var i = 0; i < 10; i++)
            {
                var newHat = new Item
                {
                    Name = product1Name,
                    Price = 19.99,
                    ProductDescription = product1Desc,
                    ProductId = product1Id,
                    SKU = Guid.NewGuid().ToString(),
                    Attributes = new List<ItemAttribute>
                    {
                        new ItemAttribute
                        {
                            Type = AttributeType.Color,
                            Value = colors[i%3]
                        },
                        new ItemAttribute
                        {
                            Type = AttributeType.Size,
                            Value = sizes[i%4]
                        },
                        new ItemAttribute
                        {
                            Type = AttributeType.Style,
                            Value = hatTypes[i%2]
                        }
                    }
                };
                hats.Add(newHat);
            }
            for (var i = 0; i < hats.Count; i++)
            {
                itemRepoMock.Setup(r => r.CreateAsync(hats[i])).ReturnsAsync(hats[i]);
            }
            hatOutcomes = new List<Item>();
            loginServiceMock.Setup(l => l.ValidatePermissionsAsync(Moq.It.IsAny<Account>(), PermissionType.Admin)).ReturnsAsync(true);
            productRepoMock.Setup(p => p.GetByProductIdAsync(product1Id)).ReturnsAsync(new Product
            {
                Name = product1Name,
                Price = product1Price,
                ProductDescription = product1Desc,
                ProductId = product1Id,
            });
            for(var i=0; i<10; i++)
            {
                itemRepoMock.Setup(p => p.CreateAsync(Moq.It.IsAny<Item>())).ReturnsAsync(hats[i]);
            }
            productService = new ProductService(productRepoMock.Object, itemRepoMock.Object, accountRepoMock.Object, loginServiceMock.Object);
        };

        Because of = () =>
        {
            for (var i = 0; i < hats.Count; i++) 
            {
                var hat = productService.CreateItemAsync(account1.Username, product1Id, hats[i]).GetAwaiter().GetResult();
                hatOutcomes.Add(hat);
            }
            hatOutcomes.OrderBy(o => o.SKU).ToList();
            hats.OrderBy(o => o.SKU).ToList();
        };

        It Should_Validate_Admin_Permission = () =>
        {
            loginServiceMock.Verify(l => l.ValidatePermissionsAsync(Moq.It.IsAny<Account>(), Moq.It.IsAny<PermissionType>()), Times.Exactly(hats.Count));
        };

        It Should_Return_Item = () =>
        {
            for (var i = 0; i < hats.Count; i++)
            {
                hatOutcomes[i].Name.ShouldEqual(hats[i].Name);
                hatOutcomes[i].Price.ShouldEqual(hats[i].Price);
                hatOutcomes[i].ProductDescription.ShouldEqual(hats[i].ProductDescription);
                hatOutcomes[i].SKU.ShouldNotEqual(Guid.Empty.ToString());
                hatOutcomes[i].Attributes.OrderBy(o => o.Type);
                var outAttributes = hats.Where(h => h.SKU == hatOutcomes[i].SKU).FirstOrDefault().Attributes.OrderBy(o => o.Type).ToList();
                for(var j=0; j<outAttributes.Count; j++)
                {
                    hatOutcomes[i].Attributes[j].Value.ShouldEqual(outAttributes[j].Value);
                }
            }
        };

        It Should_Find_Product_In_Repo = () =>
        {
            productRepoMock.Verify(r => r.GetByProductIdAsync(product1Id), Times.Exactly(hats.Count));
        };

        It Should_Persist_Item_In_Repo = () =>
        {
            itemRepoMock.Verify(r => r.CreateAsync(Moq.It.IsAny<Item>()),Times.Exactly(hats.Count));
        };

        private static IProductService productService;
        private static List<string> colors;
        private static List<string> sizes;
        private static List<string> hatTypes;
        private static List<string> prod1skus;
        private static List<Item> hats;
        private static List<string> shirtTypes;
        private static List<string> prod2Skus;
        private static List<Item> shirts;
        private static List<Item> hatOutcomes;
        private static List<Item> shirtOutcomes;
    }

    public class When_Creating_Items_For_A_Product : With_ProductRepo_Setup
    {
        Establish context = () =>
        {
            attributes = new List<ItemAttribute>
            {
                new ItemAttribute
                {
                    Type = AttributeType.Color,
                    Value = "Red"
                },
                new ItemAttribute
                {
                    Type= AttributeType.Size,
                    Value = "L"
                },
                new ItemAttribute
                {
                    Type = AttributeType.Style,
                    Value = "Fedora"
                }
            };
            itemQuantity = 10;
            hats = new List<Item>();
            for (var i = 0; i < itemQuantity; i++)
            {
                var newHat = new Item
                {
                    Name = product1Name,
                    Price = product1Price,
                    ProductDescription = product1Desc,
                    ProductId = product1Id,
                    SKU = Guid.NewGuid().ToString(),
                    Attributes = attributes
                };
                hats.Add(newHat);
            }
            for (var i = 0; i < hats.Count; i++)
            {
                itemRepoMock.Setup(r => r.CreateAsync(hats[i])).ReturnsAsync(hats[i]);
            }
            hatOutcomes = new List<Item>();
            loginServiceMock.Setup(l => l.ValidatePermissionsAsync(Moq.It.IsAny<Account>(), PermissionType.Admin)).ReturnsAsync(true);
            productRepoMock.Setup(p => p.GetByNameAsync(product1Name)).ReturnsAsync(new Product
            {
                Name = product1Name,
                Price = product1Price,
                ProductDescription = product1Desc,
                ProductId = product1Id,
            });
            for (var i = 0; i < itemQuantity; i++)
            {
                itemRepoMock.Setup(p => p.CreateAsync(Moq.It.IsAny<Item>())).ReturnsAsync(hats[i]);
            }
            productService = new ProductService(productRepoMock.Object, itemRepoMock.Object, accountRepoMock.Object, loginServiceMock.Object);
        };

        Because of = () => hatOutcomes = productService.CreateManyItemsAsync(account1.Username, product1Name, itemQuantity, attributes).GetAwaiter().GetResult().ToList();

        It Should_Validate_Admin_Permission = () =>
        {
            loginServiceMock.Verify(l => l.ValidatePermissionsAsync(Moq.It.IsAny<Account>(), Moq.It.IsAny<PermissionType>()), Times.Once);
        };

        It Should_Return_Item = () =>
        {
            for (var i = 0; i < itemQuantity; i++)
            {
                hatOutcomes[i].Name.ShouldEqual(hats[i].Name);
                hatOutcomes[i].Price.ShouldEqual(hats[i].Price);
                hatOutcomes[i].ProductDescription.ShouldEqual(hats[i].ProductDescription);
                hatOutcomes[i].SKU.ShouldNotEqual(Guid.Empty.ToString());
                hatOutcomes[i].Attributes.Select(s => s.Value).ShouldContain(attributes[0].Value);
                hatOutcomes[i].Attributes.Select(s => s.Value).ShouldContain(attributes[1].Value);
                hatOutcomes[i].Attributes.Select(s => s.Value).ShouldContain(attributes[2].Value);
            }
        };

        It Should_Find_Product_In_Repo = () =>
        {
            productRepoMock.Verify(r => r.GetByNameAsync(product1Name), Times.Once);
        };

        It Should_Persist_Item_In_Repo = () =>
        {
            itemRepoMock.Verify(r => r.CreateAsync(Moq.It.IsAny<Item>()), Times.Exactly(itemQuantity));
        };

        private static IProductService productService;
        private static List<string> colors;
        private static List<string> sizes;
        private static List<string> hatTypes;
        private static List<ItemAttribute> attributes;
        private static int itemQuantity;
        private static List<string> prod1skus;
        private static List<Item> hats;
        private static List<string> shirtTypes;
        private static List<string> prod2Skus;
        private static List<Item> shirts;
        private static List<Item> hatOutcomes;
        private static List<Item> shirtOutcomes;
    }

    public class When_Getting_All_Items_For_A_Product : With_ProductRepo_Setup
    {
        Establish context = () =>
        {
            colors = new List<string>
            {
                "Red","Green","Black"
            };
            sizes = new List<string>
            {
                "S","M","L","XL"
            };
            hatTypes = new List<string>
            {
                "Ballcap","Fedora"
            };
            hats = new List<Item>();
            for (var i = 0; i < 10; i++)
            {
                var newHat = new Item
                {
                    Name = product1Name,
                    Price = 19.99,
                    ProductDescription = product1Desc,
                    ProductId = product1Id,
                    SKU = Guid.NewGuid().ToString(),
                    Attributes = new List<ItemAttribute>
                    {
                        new ItemAttribute
                        {
                            Type = AttributeType.Color,
                            Value = colors[i%3]
                        },
                        new ItemAttribute
                        {
                            Type = AttributeType.Size,
                            Value = sizes[i%4]
                        },
                        new ItemAttribute
                        {
                            Type = AttributeType.Style,
                            Value = hatTypes[i%2]
                        }
                    }
                };
                hats.Add(newHat);
            }
            for (var i = 0; i < hats.Count; i++)
            {
                itemRepoMock.Setup(r => r.CreateAsync(hats[i])).ReturnsAsync(hats[i]);
            }
            hatOutcomes = new List<Item>();
            itemRepoMock.Setup(p => p.GetAllItemsAsync(product1Id)).ReturnsAsync(hats);
            productService = new ProductService(productRepoMock.Object, itemRepoMock.Object, accountRepoMock.Object, loginServiceMock.Object);
        };

        Because of = () =>
        {
            hatOutcomes = productService.GetItemsAsync(product1Id).GetAwaiter().GetResult().ToList();
        };

        It Should_Return_All_Items_For_The_Product = () =>
        {
            for (var i = 0; i < hats.Count; i++)
            {
                hatOutcomes[i].Name.ShouldEqual(hats[i].Name);
                hatOutcomes[i].ProductId.ShouldEqual(hats[i].ProductId);
                hatOutcomes[i].Price.ShouldEqual(hats[i].Price);
                hatOutcomes[i].ProductDescription.ShouldEqual(hats[i].ProductDescription);
                hatOutcomes[i].SKU.ShouldEqual(hats[i].SKU);
                var outAttributes = hats.Where(h => h.SKU == hatOutcomes[i].SKU).FirstOrDefault().Attributes.OrderBy(o => o.Type).ToList();
                for (var j = 0; j < outAttributes.Count; j++)
                {
                    hatOutcomes[i].Attributes[j].Value.ShouldEqual(outAttributes[j].Value);
                }
            }
        };

        It Should_Get_The_Items_From_The_Repo = () => itemRepoMock.Verify(i => i.GetAllItemsAsync(product1Id), Times.Once());

        private static List<string> colors;
        private static List<string> sizes;
        private static List<string> hatTypes;
        private static List<Item> hats;
        private static List<Item> hatOutcomes;
        private static ProductService productService;
    }

    public class When_Getting_All_Attributes_For_A_Product : With_ProductRepo_Setup
    {
        Establish context = () =>
        {
            colors = new List<string>
            {
                "Red","Green","Black"
            };
            sizes = new List<string>
            {
                "S","M","L","XL"
            };
            hatTypes = new List<string>
            {
                "Ballcap","Fedora"
            };
            attributeTypes = new List<AttributeType>
            {
                AttributeType.Color,
                AttributeType.Size,
                AttributeType.Style
            };
            attributeValues = new List<string>
            {
                "Red","Green","Black",
                "S","M","L","XL",
                "Ballcap","Fedora"
            };
            attributes = new List<ItemAttribute>
            {
                new ItemAttribute
                {
                    Type = AttributeType.Color,
                    Value = "Red"
                },
                new ItemAttribute
                {
                    Type = AttributeType.Color,
                    Value = "Green"
                },
                new ItemAttribute
                {
                    Type = AttributeType.Color,
                    Value = "Black"
                },
                new ItemAttribute
                {
                    Type = AttributeType.Size,
                    Value = "S"
                },
                new ItemAttribute
                {
                    Type= AttributeType.Size,
                    Value = "M"
                },
                new ItemAttribute
                {
                    Type= AttributeType.Size,
                    Value = "L"
                },
                new ItemAttribute
                {
                    Type= AttributeType.Size,
                    Value = "XL"
                },
                new ItemAttribute
                {
                    Type = AttributeType.Style,
                    Value = "Ballcap"
                },
                new ItemAttribute
                {
                    Type = AttributeType.Style,
                    Value = "Fedora"
                }
            };
            hats = new List<Item>();
            for (var i = 0; i < 10; i++)
            {
                var newHat = new Item
                {
                    Name = product1Name,
                    Price = 19.99,
                    ProductDescription = product1Desc,
                    ProductId = product1Id,
                    SKU = Guid.NewGuid().ToString(),
                    Attributes = new List<ItemAttribute>
                    {
                        new ItemAttribute
                        {
                            Type = AttributeType.Color,
                            Value = colors[i%3]
                        },
                        new ItemAttribute
                        {
                            Type = AttributeType.Size,
                            Value = sizes[i%4]
                        },
                        new ItemAttribute
                        {
                            Type = AttributeType.Style,
                            Value = hatTypes[i%2]
                        }
                    }
                };
                hats.Add(newHat);
            }
            itemRepoMock.Setup(i => i.GetAllItemsAsync(product1Name)).ReturnsAsync(hats);
            for (var i = 0; i < attributeTypes.Count; i++)
            {
                for (var j = 0; j < attributeValues.Count; j++)
                {
                    itemRepoMock.Setup(p => p.GetByAttributeAsync(product1Name, attributeTypes[i], attributeValues[j]))
                    .ReturnsAsync(hats.Where(h => h.Attributes.Where(a => a.Value == attributeValues[j]).Any()));
                }
            }
            productService = new ProductService(productRepoMock.Object, itemRepoMock.Object, accountRepoMock.Object, loginServiceMock.Object);
            attributeOutcome = new List<GroupedAttributes>();
        };

        Because of = () => attributeOutcome = productService.GetAttributesAsync(product1Name).GetAwaiter().GetResult().ToList();

        It Should_Return_A_List_Of_Attributes = () =>
        {
            var expectations = attributes.GroupBy(attribute => attribute.Type, attribute => attribute.Value, (type, value) =>
            new {
                Type = type.ToString(),
                Value = value.ToHashSet(),
            }).ToList();
            for(var i=0; i< expectations.Count(); i++)
            {
                attributeOutcome[i].Type.ShouldEqual(expectations[i].Type);
                attributeOutcome[i].Value.ShouldEqual(expectations[i].Value);
            }
        };

        It Should_Get_Attributes_From_ItemRepo = () => itemRepoMock.Verify(r => r.GetAllItemsAsync(product1Name),Times.Once);

        private static List<string> colors;
        private static List<string> sizes;
        private static List<string> hatTypes;
        private static List<AttributeType> attributeTypes;
        private static List<string> attributeValues;
        private static List<ItemAttribute> attributes;
        private static List<Item> hats;
        private static ProductService productService;
        private static List<GroupedAttributes> attributeOutcome;
    }

    public class When_Geting_Items_By_Attribute : With_ProductRepo_Setup
    {
        Establish context = () =>
        {
            colors = new List<string>
            {
                "Red","Green","Black"
            };
            sizes = new List<string>
            {
                "S","M","L","XL"
            };
            hatTypes = new List<string>
            {
                "Ballcap","Fedora"
            };
            attributes = new List<AttributeType>
            {
                AttributeType.Color,
                AttributeType.Size,
                AttributeType.Style
            };
            attributeValues = new List<string>
            {
                "Red","Green","Black",
                "S","M","L","XL",
                "Ballcap","Fedora"
            };
            hats = new List<Item>();
            for (var i = 0; i < 10; i++)
            {
                var newHat = new Item
                {
                    Name = product1Name,
                    Price = 19.99,
                    ProductDescription = product1Desc,
                    ProductId = product1Id,
                    SKU = Guid.NewGuid().ToString(),
                    Attributes = new List<ItemAttribute>
                    {
                        new ItemAttribute
                        {
                            Type = AttributeType.Color,
                            Value = colors[i%3]
                        },
                        new ItemAttribute
                        {
                            Type = AttributeType.Size,
                            Value = sizes[i%4]
                        },
                        new ItemAttribute
                        {
                            Type = AttributeType.Style,
                            Value = hatTypes[i%2]
                        }
                    }
                };
                hats.Add(newHat);
            }
            productRepoMock.Setup(p => p.GetByNameAsync(product1Name))
            .ReturnsAsync(new Product
            {
                Name = product1Name,
                Price = product1Price,
                ProductDescription = product1Desc,
                ProductId = product1Id,
            });
            for(var i = 0; i <attributes.Count; i++)
            {
                for(var j= 0;j<attributeValues.Count; j++)
                {
                    itemRepoMock.Setup(p => p.GetByAttributeAsync(product1Name, attributes[i], attributeValues[j]))
                    .ReturnsAsync(hats.Where(h => h.Attributes.Where(a => a.Value == attributeValues[j]).Any()));
                }
            }
            productService = new ProductService(productRepoMock.Object, itemRepoMock.Object, accountRepoMock.Object, loginServiceMock.Object);
            redItems = new List<Item>();
            greenItems = new List<Item>();
            blackItems = new List<Item>();
            sizeItems = new List<Item>();
            smallItems = new List<Item>();
            mediumItems = new List<Item>();
            largeItems = new List<Item>();
            xlItems = new List<Item>();
            styleItems = new List<Item>();
            fedoraItems = new List<Item>();
            ballcapItems = new List<Item>();
        };

        Because of = () =>
        {
            redItems = productService.GetByAttributeAsync(product1Id, AttributeType.Color, "Red").GetAwaiter().GetResult();
            greenItems = productService.GetByAttributeAsync(product1Id, AttributeType.Color, "Green").GetAwaiter().GetResult();
            blackItems = productService.GetByAttributeAsync(product1Id, AttributeType.Color, "Black").GetAwaiter().GetResult();
            smallItems = productService.GetByAttributeAsync(product1Id, AttributeType.Size, "S").GetAwaiter().GetResult();
            mediumItems = productService.GetByAttributeAsync(product1Id, AttributeType.Size, "M").GetAwaiter().GetResult();
            largeItems = productService.GetByAttributeAsync(product1Id, AttributeType.Size, "L").GetAwaiter().GetResult();
            xlItems = productService.GetByAttributeAsync(product1Id, AttributeType.Size, "XL").GetAwaiter().GetResult();
            fedoraItems = productService.GetByAttributeAsync(product1Id, AttributeType.Style, "Fedora").GetAwaiter().GetResult();
            ballcapItems = productService.GetByAttributeAsync(product1Id, AttributeType.Style, "Ballcap").GetAwaiter().GetResult();
        };

        It Should_Return_All_Items_Matching_Attribute = () =>
        {
            red = hats.Where(h => h.Attributes.Where(a => a.Value == "Red").Any()).ToList();
            for(var i=0; i<redItems.Count(); i++)
            {
                redItems.ToList()[i].Attributes.ShouldContain(new ItemAttribute { Type = AttributeType.Color, Value = "Red" });
            }
            green = hats.Where(h => h.Attributes.Where(a => a.Value == "Green").Any()).ToList();
            for (var i = 0; i < redItems.Count(); i++)
            {
                greenItems.ToList()[i].Attributes.ShouldContain(new ItemAttribute { Type = AttributeType.Color, Value = "Green" });
            }
            black = hats.Where(h => h.Attributes.Where(a => a.Value == "Black").Any()).ToList();
            for (var i = 0; i < redItems.Count(); i++)
            {
                blackItems.ToList()[i].Attributes.ShouldContain(new ItemAttribute { Type = AttributeType.Color, Value = "Black" });
            }
            small = hats.Where(h => h.Attributes.Where(a => a.Value == "S").Any()).ToList();
            for (var i = 0; i < redItems.Count(); i++)
            {
                smallItems.ToList()[i].Attributes.ShouldContain(new ItemAttribute { Type = AttributeType.Color, Value = "S" });
            }
            medium = hats.Where(h => h.Attributes.Where(a => a.Value == "M").Any()).ToList();
            for (var i = 0; i < redItems.Count(); i++)
            {
                mediumItems.ToList()[i].Attributes.ShouldContain(new ItemAttribute { Type = AttributeType.Color, Value = "M" });
            }
            large = hats.Where(h => h.Attributes.Where(a => a.Value == "L").Any()).ToList();
            for (var i = 0; i < redItems.Count(); i++)
            {
                largeItems.ToList()[i].Attributes.ShouldContain(new ItemAttribute { Type = AttributeType.Color, Value = "L" });
            }
            xl = hats.Where(h => h.Attributes.Where(a => a.Value == "XL").Any()).ToList();
            for (var i = 0; i < redItems.Count(); i++)
            {
                xlItems.ToList()[i].Attributes.ShouldContain(new ItemAttribute { Type = AttributeType.Color, Value = "XL" });
            }
            fedoras = hats.Where(h => h.Attributes.Where(a => a.Value == "Fedora").Any()).ToList();
            for (var i = 0; i < redItems.Count(); i++)
            {
                fedoraItems.ToList()[i].Attributes.ShouldContain(new ItemAttribute { Type = AttributeType.Color, Value = "Fedora" });
            }
            ballcaps = hats.Where(h => h.Attributes.Where(a => a.Value == "Ballcap").Any()).ToList();
            for (var i = 0; i < redItems.Count(); i++)
            {
                ballcapItems.ToList()[i].Attributes.ShouldContain(new ItemAttribute { Type = AttributeType.Color, Value = "Ballcap" });
            }
        };

        It Should_Get_Items_From_Repo = () => itemRepoMock.Verify(i => i.GetByAttributeAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<AttributeType>(),Moq.It.IsAny<string>()), Times.Exactly(9));

        private static List<string> colors;
        private static List<string> sizes;
        private static List<string> hatTypes;
        private static List<string> attributeValues;
        private static List<AttributeType> attributes;
        private static List<Item> hats;
        private static ProductService productService;
        private static IEnumerable<Item> redItems;
        private static IEnumerable<Item> greenItems;
        private static IEnumerable<Item> blackItems;
        private static IEnumerable<Item> sizeItems;
        private static IEnumerable<Item> smallItems;
        private static IEnumerable<Item> mediumItems;
        private static IEnumerable<Item> largeItems;
        private static IEnumerable<Item> xlItems;
        private static IEnumerable<Item> styleItems;
        private static IEnumerable<Item> fedoraItems;
        private static IEnumerable<Item> ballcapItems;
        private static List<Item> red;
        private static List<Item> green;
        private static List<Item> black;
        private static List<Item> small;
        private static List<Item> medium;
        private static List<Item> large;
        private static List<Item> xl;
        private static List<Item> fedoras;
        private static List<Item> ballcaps;
    }

    public class When_Geting_Items_For_A_Product : With_ProductRepo_Setup
    {
        Establish context = () =>
        {
            colors = new List<string>
            {
                "Red","Green","Black"
            };
            sizes = new List<string>
            {
                "S","M","L","XL"
            };
            hatTypes = new List<string>
            {
                "Ballcap","Fedora"
            };
            attributes = new List<AttributeType>
            {
                AttributeType.Color,
                AttributeType.Size,
                AttributeType.Style
            };
            attributeValues = new List<string>
            {
                "Red","Green","Black",
                "S","M","L","XL",
                "Ballcap","Fedora"
            };
            hats = new List<Item>();
            for (var i = 0; i < 10; i++)
            {
                var newHat = new Item
                {
                    Name = product1Name,
                    Price = 19.99,
                    ProductDescription = product1Desc,
                    ProductId = product1Id,
                    SKU = Guid.NewGuid().ToString(),
                    Attributes = new List<ItemAttribute>
                    {
                        new ItemAttribute
                        {
                            Type = AttributeType.Color,
                            Value = colors[i%3]
                        },
                        new ItemAttribute
                        {
                            Type = AttributeType.Size,
                            Value = sizes[i%4]
                        },
                        new ItemAttribute
                        {
                            Type = AttributeType.Style,
                            Value = hatTypes[i%2]
                        }
                    }
                };
                hats.Add(newHat);
            }
            productRepoMock.Setup(p => p.GetByProductIdAsync(product1Id))
            .ReturnsAsync(new Product
            {
                Name = product1Name,
                Price = product1Price,
                ProductDescription = product1Desc,
                ProductId = product1Id,
            });
            productRepoMock.Setup(p => p.GetByNameAsync(product1Name))
            .ReturnsAsync(new Product
            {
                Name = product1Name,
                Price = product1Price,
                ProductDescription = product1Desc,
                ProductId = product1Id,
            });
            itemRepoMock.Setup(i => i.GetAllItemsAsync(product1Name)).ReturnsAsync(hats);
            productService = new ProductService(productRepoMock.Object, itemRepoMock.Object, accountRepoMock.Object, loginServiceMock.Object);
            items = new List<Item>();
        };

        Because of = () => items = productService.GetItemsAsync(product1Name).GetAwaiter().GetResult().ToList();

        It Should_Return_All_Items_For_Product = () =>
        {
            items.Count.ShouldEqual(hats.Count);
        };

        It Should_Get_Items_From_Repo = () =>
        {
            itemRepoMock.Verify(i => i.GetAllItemsAsync(Moq.It.IsAny<string>()), Times.Once);
        };

        private static List<string> colors;
        private static List<string> sizes;
        private static List<string> hatTypes;
        private static List<AttributeType> attributes;
        private static List<string> attributeValues;
        private static List<Item> hats;
        private static ProductService productService;
        private static List<Item> items;
    }

    public class When_Purchasing_An_Item : With_ProductRepo_Setup
    {
        Establish context = () =>
        {
            colors = new List<string>
            {
                "Red","Green","Black"
            };
            sizes = new List<string>
            {
                "S","M","L","XL"
            };
            hatTypes = new List<string>
            {
                "Ballcap","Fedora"
            };
            attributes = new List<AttributeType>
            {
                AttributeType.Color,
                AttributeType.Size,
                AttributeType.Style
            };
            attributeValues = new List<string>
            {
                "Red","Green","Black",
                "S","M","L","XL",
                "Ballcap","Fedora"
            };
            hats = new List<Item>();
            for (var i = 0; i < 10; i++)
            {
                var newHat = new Item
                {
                    Name = product1Name,
                    Price = 19.99,
                    ProductDescription = product1Desc,
                    ProductId = product1Id,
                    SKU = Guid.NewGuid().ToString(),
                    Attributes = new List<ItemAttribute>
                    {
                        new ItemAttribute
                        {
                            Type = AttributeType.Color,
                            Value = colors[i%3]
                        },
                        new ItemAttribute
                        {
                            Type = AttributeType.Size,
                            Value = sizes[i%4]
                        },
                        new ItemAttribute
                        {
                            Type = AttributeType.Style,
                            Value = hatTypes[i%2]
                        }
                    }
                };
                hats.Add(newHat);
            }
            productRepoMock.Setup(p => p.GetByProductIdAsync(product1Id))
            .ReturnsAsync(new Product
            {
                Name = product1Name,
                Price = product1Price,
                ProductDescription = product1Desc,
                ProductId = product1Id,
            });
            productRepoMock.Setup(p => p.GetByNameAsync(product1Name))
            .ReturnsAsync(new Product
            {
                Name = product1Name,
                Price = product1Price,
                ProductDescription = product1Desc,
                ProductId = product1Id,
            });
            loginServiceMock.Setup(l => l.ValidateTokenAsync(account1.Username,account1.Token)).ReturnsAsync(true);
            loginServiceMock.Setup(l => l.ValidatePermissionsAsync(account1,PermissionType.User)).ReturnsAsync(true);
            accountRepoMock.Setup(a => a.GetByUsernameAsync(Moq.It.IsAny<string>())).ReturnsAsync(account1);
            itemRepoMock.Setup(i => i.GetByAttributeAsync(product1Name,Moq.It.IsAny<AttributeType>(), Moq.It.IsAny<string>())).ReturnsAsync(hats);
            productService = new ProductService(productRepoMock.Object, itemRepoMock.Object, accountRepoMock.Object, loginServiceMock.Object);
            items = new List<Item>();
        };

        Because of = () => item = productService.PurchaseItem(account1.Username, account1.Token, hats[0]).GetAwaiter().GetResult();

        It Should_Return_Item_To_Purchase = () =>
        {
            hats[0].Attributes.OrderBy(o => o.Type).ToList();
            item.Name.ShouldEqual(product1Name);
            item.Attributes.OrderBy(o => o.Type).ToList();
            for (var i = 0; i < item.Attributes.Count; i++)
            {
                item.Attributes[i].Type.ShouldEqual(hats[0].Attributes[i].Type);
                item.Attributes[i].Value.ShouldEqual(hats[0].Attributes[i].Value);
            }
        };

        It Should_Validate_Account_Token = () => loginServiceMock.Verify(l => l.ValidateTokenAsync(account1.Username, account1.Token), Times.Once);

        It Should_Validate_User_Permission = () =>
        {
            loginServiceMock.Verify(l => l.ValidatePermissionsAsync(account1,PermissionType.User), Times.Once);
        };

        It Should_Delete_The_Item_From_Repo = () => itemRepoMock.Verify(r => r.DeleteItemAsync(Moq.It.IsAny<Item>()), Times.Once);

        It Should_Get_The_Account_From_Repo = () => accountRepoMock.Verify(a => a.GetByUsernameAsync(account1.Username), Times.Once);

        It Should_Add_The_Price_To_Account_Balance = () => accountRepoMock.Verify(r => r.UpdateAsync(Moq.It.IsAny<Account>()), Times.Once);

        private static List<string> colors;
        private static List<string> sizes;
        private static List<string> hatTypes;
        private static List<AttributeType> attributes;
        private static List<string> attributeValues;
        private static List<Item> hats;
        private static IProductService productService;
        private static List<Item> items;
        private static Item item;
    }
}