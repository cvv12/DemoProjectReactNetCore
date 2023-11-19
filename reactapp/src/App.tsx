import React, { useState, useEffect, useCallback } from 'react';
import { Tabs } from 'antd';
import CustomerTab from './components/CustomerTab';
import AccountTab from './components/AccountTab';
import api from './services/api';

const { TabPane } = Tabs;

interface Customer {
    customerId: number;
    customerName: string;
    nric: string;
}
interface AccountResponse {
    accounts: Account[];
    totalCount: number;
}
interface Account {
    accountId: number;
    accountNumber: number;
    accountType: string;
    balance: number;
    customerName: string;
    nric: string;
}

const App: React.FC = () => {
    const [totalAccounts, setTotalAccounts] = useState<number>(0);
    const [activeTab, setActiveTab] = useState<string>('1')
    const [customers, setCustomers] = useState<Customer[]>([]);
    const [accounts, setAccounts] = useState<Account[]>([]);
    const [currentPage, setCurrentPage] = useState<number>(1);
    const [pageSize, setPageSize] = useState<number>(3);
    const [customerSearchTerm] = useState<string>('');
    const dropdownPageSize = 10;
    const fetchCustomers = useCallback(async (page: number, searchTerm: string) => {
        console.log("fetchCustomers called", { page, searchTerm });
        try {
            const response = await api.get('/api/Customer/GetCustomers', {
                params: { pageNumber: page, pageSize: dropdownPageSize, searchTerm }
            });
            if (response.data && Array.isArray(response.data)) {
                setCustomers(response.data);
            }
        } catch (error) {
            console.error('Failed to fetch customers:', error);
        }
    }, []);

    const fetchAccounts = useCallback(async (page: number, size: number) => {
        try {
            const response = await api.get<AccountResponse>('/api/Account/GetAccounts', {
                params: { pageNumber: page, pageSize: size }
            });
            if (response.data && Array.isArray(response.data.accounts)) {
                setTotalAccounts(response.data.totalCount);
                setAccounts(response.data.accounts); 
            }
        } catch (error) {
            console.error('Failed to fetch accounts:', error);
        }
    }, []);
    const fetchCustomersWrapper = useCallback(async (page: number, searchTerm: string) => {
        await fetchCustomers(page, searchTerm);
    }, [fetchCustomers]);

    useEffect(() => {
        console.log("useEffect triggered", { activeTab, currentPage, pageSize, customerSearchTerm });
        const fetchData = async () => {
            if (activeTab === '1') {
                await fetchCustomers(currentPage, customerSearchTerm);
            } else if (activeTab === '2') {
                await fetchAccounts(currentPage, pageSize);
            }
        };
        fetchData();
    }, [activeTab, currentPage, pageSize, customerSearchTerm, fetchAccounts, fetchCustomers]);

    const onTabChange = (key: string) => {
        setActiveTab(key);
    };

    return (
        <div className="App">
            <Tabs defaultActiveKey="1" onChange={onTabChange} style={{ margin: '16px' }}>
                <TabPane tab="Customers" key="1">
                    <CustomerTab
                        customers={customers}
                    />
                </TabPane>
                <TabPane tab="Accounts" key="2">
                    <AccountTab
                        customers={customers}
                        accounts={accounts}
                        totalAccounts={totalAccounts}
                        fetchAccounts={fetchAccounts}
                        setCurrentPage={setCurrentPage}
                        setPageSize={setPageSize}
                        fetchCustomers={fetchCustomersWrapper}
                    />
                </TabPane>
            </Tabs>
        </div>
    );
};

export default App;