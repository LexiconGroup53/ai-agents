import { useEffect, useState } from 'react'
import axios from 'axios';
import './App.css'

function App() {
  const [count, setCount] = useState(0);
  const [word, setWord] = useState('');
  const [synonyms, setSynonyms] = useState([]);

  useEffect(() => {
    axios.get(`http://localhost:5131/api/synonym/${word}`)
      .then(res => setSynonyms(res.data[word]))
  }, [word]);

  useEffect(() => {
    console.log(synonyms);
  }, [synonyms]);

  return (
    <>
    <p>{synonyms?.map(item => item + ", ")}</p>
      <p onDoubleClick={() => setWord(document.getSelection().toString())}>As the two lads walked on toward the dressing tent, where the men and women performers attired themselves in the gay suits they appeared in at the public performances, peculiar sounds came from the canvas house. The noise was, as nearly as it can be shown in print, like a series of hoarse barkings, expressed by:</p>
    </>
  )
}

export default App
