import React, { Dispatch, SetStateAction, useEffect, useState } from 'react';
import { Form, Input, Button, Table, Select, Radio, message, Modal } from 'antd';
import axios, { AxiosError } from 'axios';
import api from '../services/api';
import { RadioChangeEvent } from 'antd';
import { DeleteOutlined, EditOutlined } from '@ant-design/icons';

const { useForm } = Form;
const { Option } = Select;
interface Account {
    accountId: number;
    accountNumber: number;
    accountType: string;
    balance: number;
    customerName: string;
    nric: string;
}
interface Customer {
    customerId: number;
    customerName: string;
    nric: string;
}
interface AccountTabProps {
    totalAccounts: number;
    customers: Customer[];
    accounts: Account[];
    fetchAccounts: (pageNumber: number, pageSize: number) => Promise<void>;
    setCurrentPage: Dispatch<SetStateAction<number>>;
    setPageSize: Dispatch<SetStateAction<number>>;
    fetchCustomers: (page: number, searchTerm: string) => Promise<void>;
}

interface FormValues {
    customerId: number;
    accountType: string;
    balance: number;
}
const AccountTab: React.FC<AccountTabProps> = ({
    totalAccounts, 
    customers,
    accounts,
    fetchAccounts,
    setCurrentPage,
    setPageSize,
    fetchCustomers
}) => {
    const [currentPage, setCurrentPageInternal] = useState<number>(1);
    const [pageSize, setPageSizeInternal] = useState<number>(3); 
    const [dropdownPageSize, setDropdownPageSize] = useState<number>(10); 
    const handleCustomerSearch = (value: string) => {
        fetchCustomers(1, value);
    };
    const [form] = useForm();
    const [editingAccount, setEditingAccount] = useState<Account | null>(null);
    const [formMode, setFormMode] = useState<'edit' | 'new'>('new');
    const addOrUpdateAccount = async (values: FormValues) => {
        try {
            if (formMode === 'new') {
                const payload = {
                    CustomerId: values.customerId,
                    Account: {
                        AccountType: values.accountType,
                        Balance: typeof values.balance === 'string' ? parseFloat(values.balance) : values.balance,
                    },
                };
                await api.post('/api/Account/AddAccount', payload);
                message.success('Account added successfully!');
            } else if (editingAccount) {
                const updatePayload = {
                    accountId: editingAccount.accountId,
                    balance: values.balance,
                };
                const response = await api.put('/api/Account/UpdateAccount', updatePayload);
                if (response.status === 200) {
                    message.success('Account updated successfully!');
                } 
            }

            form.resetFields();
            setEditingAccount(null);
            setFormMode('new');
            fetchAccounts(currentPage, pageSize); 
        } catch (error) {
            if (axios.isAxiosError(error) && error.response) {
                let errorMessage = "Failed to add/update account.";
                const data = error.response.data;

                if (data.errors && Array.isArray(data.errors)) {
                    errorMessage = data.errors.join(", ");
                } else if (typeof data === 'string') {
                    errorMessage = data;
                } else if (data.error) {
                    errorMessage = data.error;
                }

                message.error(errorMessage);
            } else {
                message.error('An unexpected error occurred.');
            }
            console.error('Failed to add/update account:', error);
        }
    };

    const deleteAccount = async (accountId: number) => {
        Modal.confirm({
            title: 'Are you sure you want to deactivate this account?',
            onOk: async () => {
                try {
                    await api.delete(`/api/Account/DeleteAccount/${accountId}`);
                    message.success('Account deactivated successfully!');
                    if (formMode === 'edit' && editingAccount?.accountId === accountId) {
                        setFormMode('new');
                        form.resetFields();
                        setEditingAccount(null);
                    }

                    fetchAccounts(currentPage > 1 ? currentPage - 1 : currentPage, pageSize);
                } catch (error) {
                    console.error('Failed to delete account:', error);
                    message.error('Failed to delete account.');
                }
            },
        });
    };

    const onEdit = (account: Account) => {
        setEditingAccount(account);
        setFormMode('edit');
        const selectedCustomer = customers.find((customer) => customer.nric === account.nric);
        if (selectedCustomer) {
            form.setFieldsValue({
                ...account,
                customerId: selectedCustomer.customerId
            });
        }
    };

    const onFormModeChange = (e: RadioChangeEvent) => {
        console.log(e.target.value); 
        setFormMode(e.target.value as 'edit' | 'new');
        if (e.target.value === 'new') {
            form.resetFields();
            setEditingAccount(null);
        }
    };
    const accountColumns = [
        {
            title: 'Account Id',
            dataIndex: 'accountId',
            key: 'accountId',
            width: 100
        },
        {
            title: 'Account Number',
            dataIndex: 'accountNumber',
            key: 'accountNumber',
            width: 150, 
        },
        {
            title: 'Account Type',
            dataIndex: 'accountType',
            key: 'accountType',
            render: (text: string) => text.charAt(0).toUpperCase() + text.slice(1),
            width: 120, 
        },
        {
            title: 'Balance',
            dataIndex: 'balance',
            key: 'balance',
            render: (text: number) => `$${text.toFixed(2)}`,
            width: 100, 
        },
        {
            title: 'Customer Name',
            dataIndex: 'customerName',
            key: 'customerName',
            width: 150, 
        },
        {
            title: 'NRIC',
            dataIndex: 'nric',
            key: 'nric',
            width: 120, 
        },
        {
            title: 'Action',
            key: 'action',
            render: (_: any, record: Account) => (
                <>
                    <Button
                        icon={<EditOutlined />}
                        onClick={() => onEdit(record)}
                        style={{ marginRight: 8, border: 'none' }}
                        type="text"
                    />
                    <DeleteOutlined onClick={() => deleteAccount(record.accountId)} style={{ color: 'red', cursor: 'pointer' }} />
                </>
            ),
            width: 30, 
        },
    ];
    return (
        <>
            <Radio.Group onChange={onFormModeChange} value={formMode}>
                <Radio.Button value="new">New Account</Radio.Button>
                <Radio.Button value="edit" disabled={formMode === 'new'}>Edit Account</Radio.Button>
            </Radio.Group>
            <Form form={form} onFinish={addOrUpdateAccount} layout="vertical">
                {formMode === 'edit' && (
                    <Form.Item label="Account Number">
                        <span>{editingAccount?.accountNumber}</span>
                    </Form.Item>
                )}
                <Form.Item
                    name="accountType"
                    label="Account Type"
                    rules={[{ required: true, message: 'Please select the account type!' }]}
                >
                    <Select placeholder="Select an account type" disabled={formMode === 'edit'}>
                        <Option value="SAVINGS">SAVINGS</Option>
                        <Option value="CHECKING">CHECKING</Option>
                        <Option value="CREDIT">CREDIT</Option>
                    </Select>
                </Form.Item>
                <Form.Item
                    name="balance"
                    label="Balance"
                    rules={[
                        { required: true, message: 'Please input the initial balance!' },
                        () => ({
                            validator(_, value) {
                                const numberValue = parseFloat(value);
                                if (isNaN(numberValue)) {
                                    return Promise.reject(new Error('Please enter a valid number'));
                                }
                                if (numberValue < 0) {
                                    return Promise.reject(new Error('Value must be greater than or equal to 0'));
                                }
                                if (numberValue > 1000000000) {
                                    return Promise.reject(new Error('Maximum value is 1000 million'));
                                }
                                if (!/^\d+(\.\d{1,2})?$/.test(value)) {
                                    return Promise.reject(new Error('Maximum of 2 decimal places allowed'));
                                }
                                return Promise.resolve();
                            },
                        }),
                    ]}
                >
                    <Input type="number" step="0.01" />
                </Form.Item>
                <Form.Item
                    name="customerId"
                    label="Customer"
                    rules={[{ required: true, message: 'Please select a customer!' }]}>
                    <Select
                        disabled={formMode === 'edit'}
                        showSearch
                        placeholder="Select a customer"
                        onSearch={handleCustomerSearch}
                        filterOption={false}>
                        {customers.map((customer) => (
                            <Option key={customer.customerId} value={customer.customerId}>
                                {`${customer.customerName} (${customer.nric})`}
                            </Option>
                        ))}
                    </Select>
                </Form.Item>

                <Form.Item>
                    <Button type="primary" htmlType="submit">
                        {editingAccount ? 'Update Account' : 'Add Account'}
                    </Button>
                </Form.Item>
            </Form>
            <Table
                pagination={{
                    current: currentPage,
                    pageSize: pageSize,
                    total: totalAccounts,
                    onChange: (page, pageSize) => {
                        setCurrentPageInternal(page);
                        setPageSizeInternal(pageSize);
                        fetchAccounts(page, pageSize);
                    },
                }}
                dataSource={accounts}
                columns={accountColumns}
                rowKey="accountId"
            />

        </>
    );
};

export default AccountTab;
