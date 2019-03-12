﻿import { RECEIVE_ORDERS, REQUEST_ORDERS } from '../constants/index';

const initialState = { orders: [] };

export const actionCreators = {
  requestOrders: () => async (dispatch, getState) => {

    dispatch({ type: REQUEST_ORDERS });

    const url = `api/order/all`;
    const response = await fetch(url);
    const orders = await response.json();

    dispatch({ type: RECEIVE_ORDERS, orders });
  },

  updateOrder: (orderId, isComplete) => async (dispatch, getState) => {
    let state = getState();
    let { orders } = state.orders;
    const index = orders.findIndex(obj => obj.orderId === orderId);

    orders[index].isComplete = isComplete;
    const url = `api/order`;
    const response = await fetch(url, {
      method: 'PUT',
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(orders[index])
    });

    if (response.ok) {
      dispatch({ type: RECEIVE_ORDERS, orders });
    }
  }
};

export const reducer = (state, action) => {
  state = state || initialState;

  if (action.type === REQUEST_ORDERS) {
    return {
      ...state
    };
  }

  if (action.type === RECEIVE_ORDERS) {
    return {
      ...state,
      orders: action.orders
    };
  }

  return state;
};
