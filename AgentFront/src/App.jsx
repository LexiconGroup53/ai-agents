import { useEffect, useState } from 'react'
import axios from 'axios';
import './App.css'
import {LMStudioClient} from "@lmstudio/sdk";

function App() {
  const [word, setWord] = useState('');
  const [synonyms, setSynonyms] = useState([]);

  useEffect(() => {
    axios.get(`http://localhost:5131/api/synonym/${word}`)
      .then(res => setSynonyms(res.data[word]))
  }, [word]);

  useEffect(() => {
    console.log(synonyms);
  }, [synonyms]);

  useEffect( async () => {
    const client = new LMStudioClient();

    const model = await client.llm.model("gemma-3-12b-it-qat");
    const result = await model.respond("What is the meaning of life?");
    console.log(result.content);
  }, [])

  return (
    <>
    <p>{synonyms?.map(item => item + ", ")}</p>
      <p onDoubleClick={() => setWord(document.getSelection().toString())}>As the two lads walked on toward the dressing tent, where the men and women performers attired themselves in the gay suits they appeared in at the public performances, peculiar sounds came from the canvas house. The noise was, as nearly as it can be shown in print, like a series of hoarse barkings, expressed by:</p>
    </>
  )
}

export default App
