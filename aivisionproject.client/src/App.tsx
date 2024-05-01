import { useState } from 'react';
import SearchPage from './components/SearchPage';
import PreviouslyIndexed from './components/PreviouslyIndexed';
import './App.css';

function App() {
    const [showPreviouslyIndexed, setShowPreviouslyIndexed] = useState(false);

    const toggleView = () => {
        setShowPreviouslyIndexed(!showPreviouslyIndexed);
    }

    return (
         
        <div>
            <h1>AI Video Analysis</h1>
            {showPreviouslyIndexed ? <PreviouslyIndexed /> : <SearchPage />}
        
            <button id="navButton" onClick={toggleView}>
                {showPreviouslyIndexed ? 'Search New Videos' : 'Previously Indexed Videos'}
            </button>
        </div>
    );
}

export default App;