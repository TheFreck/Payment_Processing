import { useCallback, useEffect, useState } from 'react';
import axios from 'axios';
import './App.css';
import {AccountContext, ProductContext} from './Context';
import Welcome from './Views/Welcome';
import Login from './Views/Login';
import { Button } from '@mui/material';

const viewEnum = {
    home: 0,
    account: 1,
    product: 2,
    storeFront: 3
};


function App() {
    const [view, setView] = useState(viewEnum.home);
    const [userAcct, setUserAcct] = useState({});
    
    const accountApi = axios.create({
        baseURL: `https://localhost:7299/Account`
    });

    const login = (userName,password) => {
        accountApi.post(`login/`,{
            email: userName.replace("@", "%40"),
            password: password
        })
        .then(yup => {
            setUserAcct(yup.data);
            localStorage.setItem("loginToken",yup.data.token);
            let admin = yup.data.permissions.find(p => p.type === 0);
            if(admin !== undefined){
                localStorage.setItem("permissions.admin",admin.token);
            }
            let user =  yup.data.permissions.find(p => p.type === 1);
            if(user !== undefined){
                localStorage.setItem("permissions.user",user.token);
            }
        })
        .catch(nope => console.error(nope));
    }

    const logout = () => {
        accountApi.post(`logout/${userAcct.username}`)
        .then(yup => {
            setUserAcct({});
            setView(viewEnum.home);
            localStorage.clear();
        })
        .catch(nope => console.error(nope));
    }

    const AppCallback = useCallback(() => <AccountContext.Provider value={[login,userAcct]}>
        {localStorage.getItem("loginToken") !== null && <Button onClick={logout} >Logout</Button>}
        {localStorage.getItem("loginToken") === null && <Login />}
        {localStorage.getItem("loginToken") !== null && <Welcome />}
    </AccountContext.Provider>,[userAcct,view]);
    return <AppCallback />
}

export default App;