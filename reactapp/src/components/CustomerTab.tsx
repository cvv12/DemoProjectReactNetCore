import React from 'react';
import { Form, Input, Button, Table, message } from 'antd';
import axios, { AxiosError } from 'axios';
import api from '../services/api';

interface Customer {
    customerId: number;
    customerName: string;
    nric: string; 
}
interface CustomerTabProps {
    customers: Customer[];
}
interface ErrorResponse {
    message: string;
}
const CustomerTab: React.FC<CustomerTabProps> = ({ customers }) => {
    const [form] = Form.useForm();
    const addCustomer = async (values: any) => {
        try {
            await api.post('/api/Customer/AddCustomer', values);
            message.success('Customer added successfully!');
            setTimeout(() => {window.location.reload();}, 500);

        } catch (error) {
            const axiosError = error as AxiosError;
            if (axiosError.response) {
                console.error('Server responded with error status:', axiosError.response.status);
                const errorResponse = axiosError.response.data as ErrorResponse;
                const errorMessage = errorResponse.message || 'Failed to add customer.';
                message.error(errorMessage);
            } else if (axiosError.request) {
                console.error('No response received from the server');
                message.error('No response received from the server');
            } else {
                console.error('Error setting up the request:', axiosError.message);
                message.error('Error setting up the request.');
            }
        }
    };

    const customerColumns = [
        {
            title: 'Customer ID',
            dataIndex: 'customerId',
            key: 'customerId',
        },
        {
            title: 'Name',
            dataIndex: 'customerName',
            key: 'customerName',
        },
        {
            title: 'NRIC',
            dataIndex: 'nric',
            key: 'nric',
        },
    ];

    return (
        <>
            <Form form={form} onFinish={addCustomer} layout="vertical">
                <Form.Item
                    name="customerName"
                    label="Name"
                    rules={[{ required: true, message: 'Please input the customer name!' }]}
                >
                    <Input maxLength={45} />
                </Form.Item>
                <Form.Item
                    name="nric"
                    label="NRIC"
                    rules={[{ required: true, message: 'Please input the NRIC!' }, { max: 15, message: 'NRIC can not be longer than 15 characters!' }]}
                >
                    <Input maxLength={15} />
                </Form.Item>
                <Form.Item>
                    <Button type="primary" htmlType="submit">
                        Add Customer
                    </Button>
                </Form.Item>
            </Form>
        </>
    );
};

export default CustomerTab;
