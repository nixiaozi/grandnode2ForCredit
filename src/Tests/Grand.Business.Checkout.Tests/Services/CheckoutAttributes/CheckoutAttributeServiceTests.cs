﻿using Grand.Business.Checkout.Services.CheckoutAttributes;
using Grand.Business.Core.Interfaces.Checkout.CheckoutAttributes;
using Grand.Data;
using Grand.Domain.Orders;
using Grand.Infrastructure;
using Grand.Infrastructure.Caching;
using Grand.Infrastructure.Configuration;
using Grand.Infrastructure.Events;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Grand.Business.Checkout.Tests.Services.CheckoutAttributes;

[TestClass]
public class CheckoutAttributeServiceTests
{
    private Mock<ICacheBase> _cacheMock;
    private Mock<IMediator> _mediatorMock;
    private Mock<IRepository<CheckoutAttribute>> _repositoryMock;
    private ICheckoutAttributeService _service;
    private Mock<IContextAccessor> _workContextMock;

    [TestInitialize]
    public void Init()
    {
        _cacheMock = new Mock<ICacheBase>();
        _repositoryMock = new Mock<IRepository<CheckoutAttribute>>();
        _mediatorMock = new Mock<IMediator>();
        _workContextMock = new Mock<IContextAccessor>();
        _service = new CheckoutAttributeService(_cacheMock.Object, _repositoryMock.Object, _mediatorMock.Object,
            _workContextMock.Object, new AccessControlConfig());
    }

    [TestMethod]
    public async Task InsertCheckoutAttribute_InvokeExpectedMethods()
    {
        await _service.InsertCheckoutAttribute(new CheckoutAttribute());
        _repositoryMock.Verify(c => c.InsertAsync(It.IsAny<CheckoutAttribute>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveByPrefix(It.IsAny<string>(), It.IsAny<bool>()), Times.AtLeast(1));
        _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityInserted<CheckoutAttribute>>(), default), Times.Once);
    }

    [TestMethod]
    public void InsertCheckoutAttribute_NullArgument_ThrowException()
    {
        Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _service.InsertCheckoutAttribute(null));
    }

    [TestMethod]
    public async Task UpdateCheckoutAttribute_InvokeExpectedMethods()
    {
        await _service.UpdateCheckoutAttribute(new CheckoutAttribute());
        _repositoryMock.Verify(c => c.UpdateAsync(It.IsAny<CheckoutAttribute>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveByPrefix(It.IsAny<string>(), It.IsAny<bool>()), Times.AtLeast(1));
        _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityUpdated<CheckoutAttribute>>(), default), Times.Once);
    }

    [TestMethod]
    public void UpdateCheckoutAttribute_NullArgument_ThrowException()
    {
        Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _service.UpdateCheckoutAttribute(null));
    }

    [TestMethod]
    public async Task DeleteCheckoutAttribute_InvokeExpectedMethods()
    {
        await _service.DeleteCheckoutAttribute(new CheckoutAttribute());
        _repositoryMock.Verify(c => c.DeleteAsync(It.IsAny<CheckoutAttribute>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveByPrefix(It.IsAny<string>(), It.IsAny<bool>()), Times.AtLeast(1));
        _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityDeleted<CheckoutAttribute>>(), default), Times.Once);
    }

    [TestMethod]
    public void DeleteCheckoutAttribute_NullArgument_ThrowException()
    {
        Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _service.DeleteCheckoutAttribute(null));
    }
}