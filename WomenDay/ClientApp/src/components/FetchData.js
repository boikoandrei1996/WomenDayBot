import React, { Component, Fragment } from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { actionCreators } from '../redux/reducer/index';
import './index.css';

class FetchData extends Component {
  componentWillMount() {
    this.props.requestOrders();
  }

  componentWillReceiveProps() {
    this.props.requestOrders();
  }

  handleCompleteClick = (orderId) => {
    this.props.updateOrder(orderId, true);
  }

  handleIncompleteClick = (orderId) => {
    this.props.updateOrder(orderId, false);
  }

  formatTime = (requestTime) => {
    return new Date(requestTime).toLocaleTimeString();
  }

  template = (orders) => {
    return <table>
      <thead>
        <tr>
          <th>#</th>
          <th>Client name</th>
          <th>Room</th>
          <th>Order type</th>
          <th>Comment</th>
          <th>Time</th>
          <th>Action button</th>
        </tr>
      </thead>
      <tbody>
        {orders.map((order, index) =>
          <tr key={order.orderId}>
            <td>{index + 1}</td>
            <td>{order.userData.name}</td>
            <td>{order.userData.room}</td>
            <td>{order.orderType}</td>
            <td>{order.comment}</td>
            <td>{this.formatTime(order.requestTime)}</td>
            <td>
              <button
                type="button"
                name="action"
                className={order.isComplete ? "btn waves-effect waves-light red" : "btn waves-effect waves-light"}
                onClick={() => order.isComplete ? this.handleIncompleteClick(order.orderId) : this.handleCompleteClick(order.orderId)}>
                {order.isComplete ? "Incomplete" : "Complete"}
              </button>
            </td>
          </tr>
        )}
      </tbody>
    </table>
  }

  layout = (orders) => {
    return <div className="row" id='row'>
      <div className="col s6 orders-row">
        <span>Incompleted</span>
        {this.template(orders.filter(x => !x.isComplete))}
      </div>
      <div className="col s6 orders-row">
        <span>Completed</span>
        {this.template(orders.filter(x => x.isComplete))}
      </div>
    </div>
  }

  render() {
    const { orders, className } = this.props;
    const ordersList = orders.length ? this.layout(orders) : <div className={className}>
      <span className="blue-text text-darken-2">You don't have any orders.</span>
    </div>;

    return (
      <Fragment>
        {ordersList}
      </Fragment>
    );
  }
}

export default connect(
  state => state.orders,
  dispatch => bindActionCreators(actionCreators, dispatch)
)(FetchData);
